using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.Auth;

public sealed class RegisterOneUserCommandHandlerTests
{
    private static DateTime UtcNow => DateTime.UtcNow;
    private static readonly EmailAddress ValidEmail = EmailAddress.From("user@example.com").Value;
    private const string ValidCode = "123456";
    private const string HashedCode = "hashed::123456";
    private const string ValidPassword = "Str0ng!Pass";
    private const string ValidUniqueName = "newuser";

    [Fact]
    public async Task Handle_ValidRegistration_ReturnsSuccess()
    {
        var user = CreatePreRegistrationUser();
        var verification = CreateActiveVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new RegisterOneUserCommand(
            new RegisterOneUserCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
                UniqueName = ValidUniqueName,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(user.Id, result.Value.UserId);
    }

    [Fact]
    public async Task Handle_NonexistentEmail_ReturnsVerificationCodeNotFound()
    {
        var handler = new RegisterOneUserCommandHandler(
            new NotFoundUserRepository(),
            new NotFoundRegistrationVerificationRepository(),
            new SecretHashServiceStub(),
            new FirstLoginUserProfileInitializer(new NeverFindUserProfileRepository()),
            new UnitOfWorkStub());

        var result = await handler.Handle(new RegisterOneUserCommand(
            new RegisterOneUserCommandArgs
            {
                EmailAddress = "nonexistent@example.com",
                VerificationCode = ValidCode,
                UniqueName = ValidUniqueName,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.VerificationCodeNotFound, result.Error);
    }

    [Fact]
    public async Task Handle_AlreadyCompletedRegistration_ReturnsRegistrationProfileAlreadyCompleted()
    {
        var user = CreateAlreadyRegisteredUser();
        var verification = CreateActiveVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new RegisterOneUserCommand(
            new RegisterOneUserCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
                UniqueName = ValidUniqueName,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.RegistrationProfileAlreadyCompleted, result.Error);
    }

    [Fact]
    public async Task Handle_ExpiredCode_ReturnsVerificationCodeExpired()
    {
        var user = CreatePreRegistrationUser();
        var verification = CreateExpiredVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new RegisterOneUserCommand(
            new RegisterOneUserCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
                UniqueName = ValidUniqueName,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.VerificationCodeExpired, result.Error);
    }

    [Fact]
    public async Task Handle_AlreadyUsedCode_ReturnsVerificationCodeAlreadyUsed()
    {
        var user = CreatePreRegistrationUser();
        var verification = CreateConsumedVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new RegisterOneUserCommand(
            new RegisterOneUserCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
                UniqueName = ValidUniqueName,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.VerificationCodeAlreadyUsed, result.Error);
    }

    [Fact]
    public async Task Handle_WrongCode_TracksFailedAttemptAndReturnsError()
    {
        var user = CreatePreRegistrationUser();
        var verification = CreateActiveVerification();
        var repo = new FakeRegistrationVerificationRepository(verification);
        var handler = CreateHandler(user, verification, verificationRepo: repo);

        var result = await handler.Handle(new RegisterOneUserCommand(
            new RegisterOneUserCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = "wrong-code",
                UniqueName = ValidUniqueName,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidVerificationCode, result.Error);
        Assert.Equal(1, repo.LastUpdatedVerification?.FailedAttemptCount);
    }

    [Fact]
    public async Task Handle_FailedAttemptLimitExceeded_ReturnsAttemptLimitExceeded()
    {
        var user = CreatePreRegistrationUser();
        var verification = CreateVerificationWithFailedAttempts(5);
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new RegisterOneUserCommand(
            new RegisterOneUserCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = "wrong-code",
                UniqueName = ValidUniqueName,
                Password = ValidPassword,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.VerificationCodeAttemptLimitExceeded, result.Error);
    }

    [Fact]
    public async Task Handle_EmptyPassword_ReturnsPasswordCannotBeEmpty()
    {
        var handler = CreateHandler(CreatePreRegistrationUser(), CreateActiveVerification());

        var result = await handler.Handle(new RegisterOneUserCommand(
            new RegisterOneUserCommandArgs
            {
                EmailAddress = ValidEmail.Value,
                VerificationCode = ValidCode,
                UniqueName = ValidUniqueName,
                Password = "",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.PasswordCannotBeEmpty, result.Error);
    }

    private static User CreatePreRegistrationUser()
    {
        var result = User.From(new UserConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            UniqueName = null,
            Role = UserRole.Normal,
            PasswordHash = "placeholder",
            IsEmailVerified = false,
            FirstLoginAt = null,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static User CreateAlreadyRegisteredUser()
    {
        var result = User.From(new UserConstructorArgs
        {
            Id = Guid.NewGuid(),
            EmailAddress = ValidEmail,
            UniqueName = "existing",
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

    private static RegistrationVerification CreateActiveVerification()
    {
        var result = RegistrationVerification.From(new RegistrationVerificationConstructorArgs
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

    private static RegistrationVerification CreateExpiredVerification()
    {
        var result = RegistrationVerification.From(new RegistrationVerificationConstructorArgs
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

    private static RegistrationVerification CreateConsumedVerification()
    {
        var result = RegistrationVerification.From(new RegistrationVerificationConstructorArgs
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

    private static RegistrationVerification CreateVerificationWithFailedAttempts(int count)
    {
        var result = RegistrationVerification.From(new RegistrationVerificationConstructorArgs
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

    private static RegisterOneUserCommandHandler CreateHandler(
        User user,
        RegistrationVerification verification,
        FakeRegistrationVerificationRepository? verificationRepo = null)
    {
        var userRepo = new FakeUserRepository(user);
        var secretHashService = new SecretHashServiceStub();
        verificationRepo ??= new FakeRegistrationVerificationRepository(verification);
        var initializer = new FirstLoginUserProfileInitializer(new NeverFindUserProfileRepository());
        var uow = new UnitOfWorkStub();

        return new RegisterOneUserCommandHandler(
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

        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) =>
            Task.FromResult(uniqueName == user.UniqueName
                ? Result.Success(user)
                : Result.Failure<User>(DomainError.User.NotFound));

        public Task<IResult> UpdateOneAsync(User user)
        {
            return Task.FromResult<IResult>(Result.Success());
        }

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

    private sealed class NotFoundRegistrationVerificationRepository : IRegistrationVerificationRepository
    {
        public Task<IResult> InsertOneAsync(RegistrationVerification rv) => throw new NotSupportedException();
        public Task<IResult<RegistrationVerification>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(Result.Failure<RegistrationVerification>(DomainError.Auth.VerificationCodeNotFound));
        public Task<IResult> UpdateOneAsync(RegistrationVerification rv) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    internal sealed class FakeRegistrationVerificationRepository : IRegistrationVerificationRepository
    {
        private RegistrationVerification _verification;
        public RegistrationVerification? LastUpdatedVerification { get; private set; }

        public FakeRegistrationVerificationRepository(RegistrationVerification verification)
        {
            _verification = verification;
        }

        public Task<IResult> InsertOneAsync(RegistrationVerification verification)
        {
            _verification = verification;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult<RegistrationVerification>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(emailAddress.Value == _verification.EmailAddress.Value
                ? Result.Success(_verification)
                : Result.Failure<RegistrationVerification>(DomainError.Auth.VerificationCodeNotFound));

        public Task<IResult> UpdateOneAsync(RegistrationVerification verification)
        {
            LastUpdatedVerification = verification;
            _verification = verification;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    private sealed class NeverFindUserProfileRepository : IUserProfileRepository
    {
        public Task<IResult> InsertOneAsync(UserProfile up) => Task.FromResult<IResult>(Result.Success());
        public Task<IResult<UserProfile>> FindOneByUserIdAsync(Guid userId) =>
            Task.FromResult(Result.Failure<UserProfile>(DomainError.UserProfile.NotFound));
        public Task<IResult> UpdateOneAsync(UserProfile up) => throw new NotSupportedException();
        public Task<IResult> RemoveOneByUserIdAsync(Guid userId) => throw new NotSupportedException();
    }
}
