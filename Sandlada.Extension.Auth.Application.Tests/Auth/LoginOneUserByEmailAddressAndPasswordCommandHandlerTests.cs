using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.Auth;

public sealed class LoginOneUserByEmailAddressAndPasswordCommandHandlerTests
{
    private static DateTime UtcNow => DateTime.UtcNow;
    private static readonly EmailAddress ValidEmail = EmailAddress.From("user@example.com").Value;
    private const string ValidPassword = "correct-password";
    private const string HashedPassword = "hashed::correct-password";

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsSuccess()
    {
        var user = CreateVerifiedUser();
        var attempt = CreateCleanAttempt();
        var handler = CreateHandler(user, attempt);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.UserId);
        Assert.Equal(user.EmailAddress.Value, result.Value.EmailAddress);
    }

    [Fact]
    public async Task Handle_InvalidEmail_ReturnsInvalidCredentials()
    {
        var user = CreateVerifiedUser();
        var attempt = CreateCleanAttempt();
        var handler = new LoginOneUserByEmailAddressAndPasswordCommandHandler(
            new NotFoundUserRepository(),
            new FakeSecretHashService(),
            new FakePasswordLoginAttemptRepository(attempt),
            new FirstLoginUserProfileInitializer(new NeverFindUserProfileRepository()),
            new FakeUnitOfWork());

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = "nonexistent@example.com",
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidCredentials, result.Error);
    }

    [Fact]
    public async Task Handle_WrongPassword_TracksFailedAttemptAndReturnsInvalidCredentials()
    {
        var user = CreateVerifiedUser();
        var attempt = CreateCleanAttempt();
        var repo = new FakePasswordLoginAttemptRepository(attempt);
        var handler = CreateHandler(user, attempt, attemptRepo: repo);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                Password = "wrong-password",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidCredentials, result.Error);
        Assert.True(repo.LastUpdatedAttempt is not null);
        Assert.Equal(1, repo.LastUpdatedAttempt!.FailedAttemptCount);
    }

    [Fact]
    public async Task Handle_FiveFailedAttempts_LocksAccount()
    {
        var user = CreateVerifiedUser();
        var attempt = CreateAttemptWithFailedCount(4);
        var repo = new FakePasswordLoginAttemptRepository(attempt);
        var handler = CreateHandler(user, attempt, attemptRepo: repo);

        // 5th failed attempt
        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                Password = "wrong-password",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.PasswordLoginFailedAttemptLimitExceeded, result.Error);
        Assert.True(repo.LastUpdatedAttempt is not null);
        Assert.True(repo.LastUpdatedAttempt!.LockoutEnd is not null);
        Assert.True(repo.LastUpdatedAttempt!.LockoutEnd > UtcNow);
    }

    [Fact]
    public async Task Handle_AccountLocked_ReturnsAccountLocked()
    {
        var user = CreateVerifiedUser();
        var attempt = CreateAttemptWithLockout(UtcNow.AddMinutes(15));
        var handler = CreateHandler(user, attempt);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.AccountLocked, result.Error);
    }

    [Fact]
    public async Task Handle_RequestLimitExceeded_ReturnsLimitError()
    {
        var user = CreateVerifiedUser();
        var attempt = CreateAttemptWithRequestCount(20);
        var handler = CreateHandler(user, attempt);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.PasswordLoginRequestLimitExceeded, result.Error);
    }

    [Fact]
    public async Task Handle_EmailNotVerified_ReturnsEmailNotVerified()
    {
        var user = CreateUnverifiedUser();
        var attempt = CreateCleanAttempt();
        var handler = CreateHandler(user, attempt);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.EmailAddressNotVerified, result.Error);
    }

    [Fact]
    public async Task Handle_EmptyPassword_ReturnsInvalidCredentials()
    {
        var handler = CreateHandler(CreateVerifiedUser(), CreateCleanAttempt());

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                Password = "",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidCredentials, result.Error);
    }

    [Fact]
    public async Task Handle_SuccessfulLogin_ResetsAttemptAndInitializesProfile()
    {
        var user = CreateVerifiedUser();
        var attempt = CreateAttemptWithFailedCount(2);
        var repo = new FakePasswordLoginAttemptRepository(attempt);
        var initializer = new FirstLoginUserProfileInitializer(new NeverFindUserProfileRepository());
        var handler = new LoginOneUserByEmailAddressAndPasswordCommandHandler(
            new FakeUserRepository(user),
            new FakeSecretHashService(),
            repo,
            initializer,
            new FakeUnitOfWork());

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndPasswordCommand(
            new LoginOneUserByEmailAddressAndPasswordCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        // Should reset failed attempts on success
        Assert.Equal(0, repo.LastUpdatedAttempt!.FailedAttemptCount);
        Assert.Null(repo.LastUpdatedAttempt!.LockoutEnd);
    }

    private static User CreateVerifiedUser()
    {
        var result = User.From(new UserConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            UniqueName = "user",
            Role = UserRole.Normal,
            PasswordHash = HashedPassword,
            IsEmailVerified = true,
            FirstLoginAt = UtcNow,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static User CreateUnverifiedUser()
    {
        var result = User.From(new UserConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            UniqueName = "user",
            Role = UserRole.Normal,
            PasswordHash = HashedPassword,
            IsEmailVerified = false,
            FirstLoginAt = null,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static PasswordLoginAttempt CreateCleanAttempt()
    {
        var result = PasswordLoginAttempt.From(new PasswordLoginAttemptConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            FailedAttemptCount = 0,
            LockoutEnd = null,
            RequestCount = 0,
            RequestCountDate = UtcNow.Date,
            LastFailedAttemptAt = null,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static PasswordLoginAttempt CreateAttemptWithFailedCount(int count)
    {
        var result = PasswordLoginAttempt.From(new PasswordLoginAttemptConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            FailedAttemptCount = count,
            LockoutEnd = null,
            RequestCount = 0,
            RequestCountDate = UtcNow.Date,
            LastFailedAttemptAt = count > 0 ? UtcNow.AddMinutes(-1) : null,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static PasswordLoginAttempt CreateAttemptWithRequestCount(int count)
    {
        var result = PasswordLoginAttempt.From(new PasswordLoginAttemptConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            FailedAttemptCount = 0,
            LockoutEnd = null,
            RequestCount = count,
            RequestCountDate = UtcNow.Date,
            LastFailedAttemptAt = null,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static PasswordLoginAttempt CreateAttemptWithLockout(DateTime? lockoutEnd)
    {
        var result = PasswordLoginAttempt.From(new PasswordLoginAttemptConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            FailedAttemptCount = 5,
            LockoutEnd = lockoutEnd,
            RequestCount = 4,
            RequestCountDate = UtcNow.Date,
            LastFailedAttemptAt = UtcNow.AddMinutes(-1),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static LoginOneUserByEmailAddressAndPasswordCommandHandler CreateHandler(
        User user,
        PasswordLoginAttempt attempt,
        FirstLoginUserProfileInitializer? initializer = null,
        FakePasswordLoginAttemptRepository? attemptRepo = null)
    {
        var userRepo = new FakeUserRepository(user);
        var secretHashService = new FakeSecretHashService();
        attemptRepo ??= new FakePasswordLoginAttemptRepository(attempt);
        initializer ??= new FirstLoginUserProfileInitializer(new NeverFindUserProfileRepository());
        var uow = new FakeUnitOfWork();

        return new LoginOneUserByEmailAddressAndPasswordCommandHandler(
            userRepo, secretHashService, attemptRepo, initializer, uow);
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public bool WasUpdated { get; private set; }

        public Task<IResult> InsertOneAsync(User user) => throw new NotSupportedException();

        public Task<IResult<User>> FindOneByIdAsync(Guid id) =>
            Task.FromResult(id == user.Id
                ? Result.Success(user)
                : Result.Failure<User>(DomainError.User.NotFound));

        public Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(emailAddress.Value == user.EmailAddress.Value
                ? Result.Success(user)
                : Result.Failure<User>(DomainError.User.NotFound));

        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) =>
            Task.FromResult(uniqueName == user.UniqueName
                ? Result.Success(user)
                : Result.Failure<User>(DomainError.User.NotFound));

        public Task<IResult> UpdateOneAsync(User updatedUser)
        {
            WasUpdated = true;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    private sealed class NotFoundUserRepository : IUserRepository
    {
        public Task<IResult> InsertOneAsync(User user) => throw new NotSupportedException();
        public Task<IResult<User>> FindOneByIdAsync(Guid id) =>
            Task.FromResult(Result.Failure<User>(DomainError.User.NotFound));
        public Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(Result.Failure<User>(DomainError.User.NotFound));
        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) =>
            Task.FromResult(Result.Failure<User>(DomainError.User.NotFound));
        public Task<IResult> UpdateOneAsync(User user) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    internal sealed class FakePasswordLoginAttemptRepository : IPasswordLoginAttemptRepository
    {
        private PasswordLoginAttempt _attempt;
        public PasswordLoginAttempt? LastUpdatedAttempt { get; private set; }

        public FakePasswordLoginAttemptRepository(PasswordLoginAttempt attempt)
        {
            _attempt = attempt;
        }

        public Task<IResult> InsertOneAsync(PasswordLoginAttempt attempt)
        {
            _attempt = attempt;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult<PasswordLoginAttempt>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(emailAddress.Value == _attempt.EmailAddress.Value
                ? Result.Success(_attempt)
                : Result.Failure<PasswordLoginAttempt>(DomainError.Auth.InvalidCredentials));

        public Task<IResult> UpdateOneAsync(PasswordLoginAttempt attempt)
        {
            LastUpdatedAttempt = attempt;
            _attempt = attempt;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    private sealed class NeverFindUserProfileRepository : IUserProfileRepository
    {
        public Task<IResult> InsertOneAsync(UserProfile userProfile) => throw new NotSupportedException();
        public Task<IResult<UserProfile>> FindOneByUserIdAsync(Guid userId) =>
            Task.FromResult(Result.Failure<UserProfile>(DomainError.UserProfile.NotFound));
        public Task<IResult> UpdateOneAsync(UserProfile userProfile) => throw new NotSupportedException();
        public Task<IResult> RemoveOneByUserIdAsync(Guid userId) => throw new NotSupportedException();
    }

    internal sealed class FakeSecretHashService : ISecretHashService
    {
        public string Hash(string value) => $"hashed::{value}";
        public bool Verify(string value, string hash) => hash == $"hashed::{value}";
    }

    internal sealed class FakeUnitOfWork : IApplicationUnitOfWork
    {
        public int SaveChangesCallCount { get; set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}
