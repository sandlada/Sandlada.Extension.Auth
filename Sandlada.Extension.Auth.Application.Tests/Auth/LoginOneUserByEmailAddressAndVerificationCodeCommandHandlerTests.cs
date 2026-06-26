using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.Auth;

public sealed class LoginOneUserByEmailAddressAndVerificationCodeCommandHandlerTests
{
    private static DateTime UtcNow => DateTime.UtcNow;
    private static readonly EmailAddress ValidEmail = EmailAddress.From("user@example.com").Value;
    private const string ValidCode = "123456";
    private const string HashedCode = "hashed::123456";

    [Fact]
    public async Task Handle_ValidCode_ReturnsSuccess()
    {
        var user = CreateVerifiedUser();
        var verification = CreateActiveVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndVerificationCodeCommand(
            new LoginOneUserByEmailAddressAndVerificationCodeCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.UserId);
    }

    [Fact]
    public async Task Handle_NonexistentEmail_ReturnsInvalidCredentials()
    {
        var handler = new LoginOneUserByEmailAddressAndVerificationCodeCommandHandler(
            new NotFoundUserRepository(),
            new NotFoundLoginVerificationRepository(),
            new SecretHashServiceStub(),
            new FirstLoginUserProfileInitializer(new NeverFindUserProfileRepository()),
            new UnitOfWorkStub());

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndVerificationCodeCommand(
            new LoginOneUserByEmailAddressAndVerificationCodeCommandArgs
            {
                EmailAddress = "nonexistent@example.com",
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidCredentials, result.Error);
    }

    [Fact]
    public async Task Handle_WrongCode_TracksFailedAttemptAndReturnsInvalidVerificationCode()
    {
        var user = CreateVerifiedUser();
        var verification = CreateActiveVerification();
        var repo = new FakeLoginVerificationRepository(verification);
        var handler = CreateHandler(user, verification, verificationRepo: repo);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndVerificationCodeCommand(
            new LoginOneUserByEmailAddressAndVerificationCodeCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = "wrong-code",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidVerificationCode, result.Error);
        Assert.True(repo.LastUpdatedVerification is not null);
        Assert.Equal(1, repo.LastUpdatedVerification!.FailedAttemptCount);
    }

    [Fact]
    public async Task Handle_ExpiredCode_ReturnsVerificationCodeExpired()
    {
        var user = CreateVerifiedUser();
        var verification = CreateExpiredVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndVerificationCodeCommand(
            new LoginOneUserByEmailAddressAndVerificationCodeCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.VerificationCodeExpired, result.Error);
    }

    [Fact]
    public async Task Handle_AlreadyUsedCode_ReturnsVerificationCodeAlreadyUsed()
    {
        var user = CreateVerifiedUser();
        var verification = CreateConsumedVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndVerificationCodeCommand(
            new LoginOneUserByEmailAddressAndVerificationCodeCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.VerificationCodeAlreadyUsed, result.Error);
    }

    [Fact]
    public async Task Handle_FailedAttemptLimitExceeded_ReturnsAttemptLimitExceeded()
    {
        var user = CreateVerifiedUser();
        var verification = CreateVerificationWithFailedAttempts(5);
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndVerificationCodeCommand(
            new LoginOneUserByEmailAddressAndVerificationCodeCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = "wrong-code",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.VerificationCodeAttemptLimitExceeded, result.Error);
    }

    [Fact]
    public async Task Handle_UnverifiedEmail_ReturnsEmailNotVerified()
    {
        var user = CreateUnverifiedUser();
        var verification = CreateActiveVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndVerificationCodeCommand(
            new LoginOneUserByEmailAddressAndVerificationCodeCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.EmailAddressNotVerified, result.Error);
    }

    [Fact]
    public async Task Handle_EmptyCode_ReturnsInvalidVerificationCode()
    {
        var handler = CreateHandler(CreateVerifiedUser(), CreateActiveVerification());

        var result = await handler.Handle(new LoginOneUserByEmailAddressAndVerificationCodeCommand(
            new LoginOneUserByEmailAddressAndVerificationCodeCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = "",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidVerificationCode, result.Error);
    }

    private static User CreateVerifiedUser()
    {
        var result = User.From(new UserConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            UniqueName = "user",
            Role = UserRole.Normal,
            PasswordHash = "hash",
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
            PasswordHash = "hash",
            IsEmailVerified = false,
            FirstLoginAt = null,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static LoginVerification CreateActiveVerification()
    {
        var result = LoginVerification.From(new LoginVerificationConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            VerificationCodeHash = HashedCode,
            ExpiresAt = UtcNow.AddHours(1),
            FailedAttemptCount = 0,
            RequestCount = 1,
            RequestCountDate = UtcNow.Date,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            ConsumedAt = null,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static LoginVerification CreateExpiredVerification()
    {
        var result = LoginVerification.From(new LoginVerificationConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            VerificationCodeHash = HashedCode,
            ExpiresAt = UtcNow.AddHours(-1),
            FailedAttemptCount = 0,
            RequestCount = 1,
            RequestCountDate = UtcNow.Date,
            CreatedAt = UtcNow.AddHours(-2),
            UpdatedAt = UtcNow.AddHours(-1),
            ConsumedAt = null,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static LoginVerification CreateConsumedVerification()
    {
        var result = LoginVerification.From(new LoginVerificationConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            VerificationCodeHash = HashedCode,
            ExpiresAt = UtcNow.AddHours(1),
            FailedAttemptCount = 0,
            RequestCount = 1,
            RequestCountDate = UtcNow.Date,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            ConsumedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static LoginVerification CreateVerificationWithFailedAttempts(int count)
    {
        var result = LoginVerification.From(new LoginVerificationConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            VerificationCodeHash = HashedCode,
            ExpiresAt = UtcNow.AddHours(1),
            FailedAttemptCount = count,
            RequestCount = 1,
            RequestCountDate = UtcNow.Date,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            ConsumedAt = null,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static LoginOneUserByEmailAddressAndVerificationCodeCommandHandler CreateHandler(
        User user,
        LoginVerification verification,
        FirstLoginUserProfileInitializer? initializer = null,
        FakeLoginVerificationRepository? verificationRepo = null)
    {
        var userRepo = new FakeUserRepository(user);
        var secretHashService = new SecretHashServiceStub();
        verificationRepo ??= new FakeLoginVerificationRepository(verification);
        initializer ??= new FirstLoginUserProfileInitializer(new NeverFindUserProfileRepository());
        var uow = new UnitOfWorkStub();

        return new LoginOneUserByEmailAddressAndVerificationCodeCommandHandler(
            userRepo, verificationRepo, secretHashService, initializer, uow);
    }

    private sealed class SecretHashServiceStub : ISecretHashService
    {
        public string Hash(string value) => $"hashed::{value}";
        public bool Verify(string value, string hash) => hash == $"hashed::{value}";
    }

    private sealed class UnitOfWorkStub : IApplicationUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) => Task.FromResult(1);
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
        public Task<IResult> InsertOneAsync(User user) => throw new NotSupportedException();
        public Task<IResult<User>> FindOneByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(emailAddress.Value == user.EmailAddress.Value
                ? Result.Success(user)
                : Result.Failure<User>(DomainError.User.NotFound));
        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) => throw new NotSupportedException();
        public Task<IResult> UpdateOneAsync(User user) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    private sealed class NotFoundUserRepository : IUserRepository
    {
        public Task<IResult> InsertOneAsync(User user) => throw new NotSupportedException();
        public Task<IResult<User>> FindOneByIdAsync(Guid id) => throw new NotSupportedException();
        public Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(Result.Failure<User>(DomainError.User.NotFound));
        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) => throw new NotSupportedException();
        public Task<IResult> UpdateOneAsync(User user) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    private sealed class NotFoundLoginVerificationRepository : ILoginVerificationRepository
    {
        public Task<IResult> InsertOneAsync(LoginVerification loginVerification) => throw new NotSupportedException();
        public Task<IResult<LoginVerification>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(Result.Failure<LoginVerification>(DomainError.Auth.VerificationCodeNotFound));
        public Task<IResult> UpdateOneAsync(LoginVerification loginVerification) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    internal sealed class FakeLoginVerificationRepository : ILoginVerificationRepository
    {
        private LoginVerification _verification;
        public LoginVerification? LastUpdatedVerification { get; private set; }

        public FakeLoginVerificationRepository(LoginVerification verification)
        {
            _verification = verification;
        }

        public Task<IResult> InsertOneAsync(LoginVerification verification)
        {
            _verification = verification;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult<LoginVerification>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(emailAddress.Value == _verification.EmailAddress.Value
                ? Result.Success(_verification)
                : Result.Failure<LoginVerification>(DomainError.Auth.VerificationCodeNotFound));

        public Task<IResult> UpdateOneAsync(LoginVerification verification)
        {
            LastUpdatedVerification = verification;
            _verification = verification;
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
}
