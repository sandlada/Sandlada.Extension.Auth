using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.Auth;

public sealed class ConfirmEmailRebindCommandHandlerTests
{
    private static DateTime UtcNow => DateTime.UtcNow;
    private static readonly EmailAddress TargetEmail = EmailAddress.From("newemail@example.com").Value;
    private static readonly Guid UserId = Guid.NewGuid();
    private const string ValidCode = "123456";
    private const string HashedCode = "hashed::123456";

    [Fact]
    public async Task Handle_ValidCode_UpdatesEmailAndReturnsSuccess()
    {
        var user = CreateVerifiedUser();
        var verification = CreateActiveVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new ConfirmEmailRebindCommand(
            UserId,
            new ConfirmEmailRebindCommandArgs
            {
                EmailAddress = TargetEmail.Value,
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserId, result.Value.UserId);
        Assert.Equal(TargetEmail.Value, result.Value.EmailAddress);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        var handler = new ConfirmEmailRebindCommandHandler(
            new NotFoundUserRepository(),
            new NotFoundEmailRebindVerificationRepository(),
            new FakeSecretHashService(),
            new FakeUnitOfWork());

        var result = await handler.Handle(new ConfirmEmailRebindCommand(
            Guid.NewGuid(),
            new ConfirmEmailRebindCommandArgs
            {
                EmailAddress = TargetEmail.Value,
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.User.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_UnverifiedEmail_ReturnsEmailNotVerified()
    {
        var user = CreateUnverifiedUser();
        var verification = CreateActiveVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new ConfirmEmailRebindCommand(
            UserId,
            new ConfirmEmailRebindCommandArgs
            {
                EmailAddress = TargetEmail.Value,
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.EmailAddressNotVerified, result.Error);
    }

    [Fact]
    public async Task Handle_WrongTargetEmail_ReturnsInvalidVerificationCode()
    {
        var user = CreateVerifiedUser();
        var verification = CreateActiveVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new ConfirmEmailRebindCommand(
            UserId,
            new ConfirmEmailRebindCommandArgs
            {
                EmailAddress = "wrong@example.com",
                VerificationCode = ValidCode,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidVerificationCode, result.Error);
    }

    [Fact]
    public async Task Handle_WrongCode_TracksFailedAttemptAndReturnsError()
    {
        var user = CreateVerifiedUser();
        var verification = CreateActiveVerification();
        var repo = new FakeEmailRebindVerificationRepository(verification);
        var handler = CreateHandler(user, verification, verificationRepo: repo);

        var result = await handler.Handle(new ConfirmEmailRebindCommand(
            UserId,
            new ConfirmEmailRebindCommandArgs
            {
                EmailAddress = TargetEmail.Value,
                VerificationCode = "wrong-code",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.InvalidVerificationCode, result.Error);
        Assert.Equal(1, repo.LastUpdatedVerification?.FailedAttemptCount);
    }

    [Fact]
    public async Task Handle_ExpiredCode_ReturnsVerificationCodeExpired()
    {
        var user = CreateVerifiedUser();
        var verification = CreateExpiredVerification();
        var handler = CreateHandler(user, verification);

        var result = await handler.Handle(new ConfirmEmailRebindCommand(
            UserId,
            new ConfirmEmailRebindCommandArgs
            {
                EmailAddress = TargetEmail.Value,
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

        var result = await handler.Handle(new ConfirmEmailRebindCommand(
            UserId,
            new ConfirmEmailRebindCommandArgs
            {
                EmailAddress = TargetEmail.Value,
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

        var result = await handler.Handle(new ConfirmEmailRebindCommand(
            UserId,
            new ConfirmEmailRebindCommandArgs
            {
                EmailAddress = TargetEmail.Value,
                VerificationCode = "wrong-code",
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.VerificationCodeAttemptLimitExceeded, result.Error);
    }

    private static User CreateVerifiedUser()
    {
        var result = User.From(new UserConstructorArgs
        {
            Id = UserId,
            EmailAddress = EmailAddress.From("old@example.com").Value,
            UniqueName = "user",
            Role = UserRole.Normal,
            PasswordHash = "hash",
            IsEmailVerified = true,
            FirstLoginAt = null,
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
            Id = UserId,
            EmailAddress = EmailAddress.From("old@example.com").Value,
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

    private static EmailRebindVerification CreateActiveVerification()
    {
        var result = EmailRebindVerification.From(new EmailRebindVerificationConstructorArgs
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            TargetEmailAddress = TargetEmail,
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

    private static EmailRebindVerification CreateExpiredVerification()
    {
        var result = EmailRebindVerification.From(new EmailRebindVerificationConstructorArgs
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            TargetEmailAddress = TargetEmail,
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

    private static EmailRebindVerification CreateConsumedVerification()
    {
        var result = EmailRebindVerification.From(new EmailRebindVerificationConstructorArgs
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            TargetEmailAddress = TargetEmail,
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

    private static EmailRebindVerification CreateVerificationWithFailedAttempts(int count)
    {
        var result = EmailRebindVerification.From(new EmailRebindVerificationConstructorArgs
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            TargetEmailAddress = TargetEmail,
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

    private static ConfirmEmailRebindCommandHandler CreateHandler(
        User user,
        EmailRebindVerification verification,
        FakeEmailRebindVerificationRepository? verificationRepo = null)
    {
        var userRepo = new FakeUserRepository(user);
        var secretHashService = new FakeSecretHashService();
        verificationRepo ??= new FakeEmailRebindVerificationRepository(verification);
        var uow = new FakeUnitOfWork();

        return new ConfirmEmailRebindCommandHandler(
            userRepo, verificationRepo, secretHashService, uow);
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

        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) => throw new NotSupportedException();

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
        public Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress) => throw new NotSupportedException();
        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) => throw new NotSupportedException();
        public Task<IResult> UpdateOneAsync(User user) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    internal sealed class FakeEmailRebindVerificationRepository : IEmailRebindVerificationRepository
    {
        private EmailRebindVerification _verification;
        public EmailRebindVerification? LastUpdatedVerification { get; private set; }

        public FakeEmailRebindVerificationRepository(EmailRebindVerification verification)
        {
            _verification = verification;
        }

        public Task<IResult> InsertOneAsync(EmailRebindVerification verification)
        {
            _verification = verification;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult<EmailRebindVerification>> FindOneByUserIdAsync(Guid userId) =>
            Task.FromResult(userId == _verification.UserId
                ? Result.Success(_verification)
                : Result.Failure<EmailRebindVerification>(DomainError.Auth.EmailRebindVerificationNotFound));

        public Task<IResult> UpdateOneAsync(EmailRebindVerification verification)
        {
            LastUpdatedVerification = verification;
            _verification = verification;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    internal sealed class NotFoundEmailRebindVerificationRepository : IEmailRebindVerificationRepository
    {
        public Task<IResult> InsertOneAsync(EmailRebindVerification verification) => throw new NotSupportedException();
        public Task<IResult<EmailRebindVerification>> FindOneByUserIdAsync(Guid userId) =>
            Task.FromResult(Result.Failure<EmailRebindVerification>(DomainError.Auth.EmailRebindVerificationNotFound));
        public Task<IResult> UpdateOneAsync(EmailRebindVerification verification) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
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
