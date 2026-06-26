using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.Auth;

public sealed class RequestEmailRebindVerificationCodeCommandHandlerTests
{
    private static DateTime UtcNow => DateTime.UtcNow;
    private static readonly EmailAddress CurrentEmail = EmailAddress.From("user@example.com").Value;
    private static readonly EmailAddress TargetEmail = EmailAddress.From("newemail@example.com").Value;
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ValidRequest_ReturnsSuccess()
    {
        var user = CreateVerifiedUser();
        var handler = CreateHandler(user, verificationExists: false);

        var result = await handler.Handle(new RequestEmailRebindVerificationCodeCommand(
            UserId,
            new RequestEmailRebindVerificationCodeCommandArgs
            {
                EmailAddress = TargetEmail.Value,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TargetEmail.Value, result.Value.EmailAddress);
        Assert.True(result.Value.ExpiresAt > UtcNow);
    }

    [Fact]
    public async Task Handle_SameEmail_ReturnsEmailAddressUnchanged()
    {
        var user = CreateVerifiedUser();
        var handler = CreateHandler(user, verificationExists: false);

        var result = await handler.Handle(new RequestEmailRebindVerificationCodeCommand(
            UserId,
            new RequestEmailRebindVerificationCodeCommandArgs
            {
                EmailAddress = CurrentEmail.Value,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.EmailAddressUnchanged, result.Error);
    }

    [Fact]
    public async Task Handle_EmailAlreadyUsedByAnotherUser_ReturnsEmailAddressAlreadyExists()
    {
        var user = CreateVerifiedUser();
        var handler = new RequestEmailRebindVerificationCodeCommandHandler(
            new EmailTakenUserRepository(),
            new NotFoundEmailRebindVerificationRepository(),
            new FixedCodeGenerator(),
            new FakeVerificationCodeSender(),
            new FakeSecretHashService(),
            new FakeUnitOfWork());

        var result = await handler.Handle(new RequestEmailRebindVerificationCodeCommand(
            UserId,
            new RequestEmailRebindVerificationCodeCommandArgs
            {
                EmailAddress = TargetEmail.Value,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.User.EmailAddressAlreadyExists, result.Error);
    }

    [Fact]
    public async Task Handle_UserNotFound_ReturnsNotFound()
    {
        var handler = new RequestEmailRebindVerificationCodeCommandHandler(
            new NotFoundUserRepository(),
            new NotFoundEmailRebindVerificationRepository(),
            new FixedCodeGenerator(),
            new FakeVerificationCodeSender(),
            new FakeSecretHashService(),
            new FakeUnitOfWork());

        var result = await handler.Handle(new RequestEmailRebindVerificationCodeCommand(
            Guid.NewGuid(),
            new RequestEmailRebindVerificationCodeCommandArgs
            {
                EmailAddress = TargetEmail.Value,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.User.NotFound, result.Error);
    }

    [Fact]
    public async Task Handle_UnverifiedEmail_ReturnsEmailNotVerified()
    {
        var user = CreateUnverifiedUser();
        var handler = CreateHandler(user, verificationExists: false);

        var result = await handler.Handle(new RequestEmailRebindVerificationCodeCommand(
            UserId,
            new RequestEmailRebindVerificationCodeCommandArgs
            {
                EmailAddress = TargetEmail.Value,
            }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.Auth.EmailAddressNotVerified, result.Error);
    }

    [Fact]
    public async Task Handle_ExistingVerification_RenewsCode()
    {
        var user = CreateVerifiedUser();
        var existingVerification = CreateActiveVerification();
        var handler = CreateHandler(user, verificationExists: true, existingVerification: existingVerification);

        var result = await handler.Handle(new RequestEmailRebindVerificationCodeCommand(
            UserId,
            new RequestEmailRebindVerificationCodeCommandArgs
            {
                EmailAddress = TargetEmail.Value,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(TargetEmail.Value, result.Value.EmailAddress);
    }

    private static User CreateVerifiedUser()
    {
        var result = User.From(new UserConstructorArgs
        {
            Id = UserId,
            EmailAddress = CurrentEmail,
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
            EmailAddress = CurrentEmail,
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
            VerificationCodeHash = "old-hash",
            ExpiresAt = UtcNow.AddMinutes(-1),
            FailedAttemptCount = 0,
            RequestCount = 1,
            RequestCountDate = UtcNow.Date,
            CreatedAt = UtcNow.AddHours(-1),
            UpdatedAt = UtcNow.AddMinutes(-1),
            ConsumedAt = null,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static RequestEmailRebindVerificationCodeCommandHandler CreateHandler(
        User user,
        bool verificationExists,
        EmailRebindVerification? existingVerification = null)
    {
        var userRepo = new FakeUserRepository(user);
        IEmailRebindVerificationRepository verificationRepo = verificationExists
            ? new FakeEmailRebindVerificationRepository(existingVerification!)
            : new NotFoundEmailRebindVerificationRepository();
        var codeGenerator = new FixedCodeGenerator();
        var codeSender = new FakeVerificationCodeSender();
        var secretHashService = new FakeSecretHashService();
        var uow = new FakeUnitOfWork();

        return new RequestEmailRebindVerificationCodeCommandHandler(
            userRepo, verificationRepo, codeGenerator, codeSender, secretHashService, uow);
    }

    private sealed class FakeUserRepository(User user) : IUserRepository
    {
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
        public Task<IResult> UpdateOneAsync(User user) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    private sealed class EmailTakenUserRepository : IUserRepository
    {
        public Task<IResult> InsertOneAsync(User user) => throw new NotSupportedException();

        public Task<IResult<User>> FindOneByIdAsync(Guid id) =>
            Task.FromResult(id == UserId
                ? Result.Success(User.From(new UserConstructorArgs
                {
                    Id = UserId,
                    EmailAddress = CurrentEmail,
                    UniqueName = "user",
                    Role = UserRole.Normal,
                    PasswordHash = "hash",
                    IsEmailVerified = true,
                    FirstLoginAt = null,
                    CreatedAt = UtcNow,
                    UpdatedAt = UtcNow,
                }).Value)
                : Result.Failure<User>(DomainError.User.NotFound));

        // Simulate another user already using the target email
        public Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress) =>
            Task.FromResult(emailAddress.Value == TargetEmail.Value
                ? Result.Success(User.From(new UserConstructorArgs
                {
                    Id = Guid.NewGuid(),
                    EmailAddress = TargetEmail,
                    UniqueName = "other",
                    Role = UserRole.Normal,
                    PasswordHash = "hash",
                    IsEmailVerified = true,
                    FirstLoginAt = null,
                    CreatedAt = UtcNow,
                    UpdatedAt = UtcNow,
                }).Value)
                : Result.Failure<User>(DomainError.User.NotFound));

        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) => throw new NotSupportedException();
        public Task<IResult> UpdateOneAsync(User user) => throw new NotSupportedException();
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

    private sealed class NotFoundEmailRebindVerificationRepository : IEmailRebindVerificationRepository
    {
        public Task<IResult> InsertOneAsync(EmailRebindVerification verification) =>
            Task.FromResult<IResult>(Result.Success());
        public Task<IResult<EmailRebindVerification>> FindOneByUserIdAsync(Guid userId) =>
            Task.FromResult(Result.Failure<EmailRebindVerification>(DomainError.Auth.EmailRebindVerificationNotFound));
        public Task<IResult> UpdateOneAsync(EmailRebindVerification verification) => throw new NotSupportedException();
        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    internal sealed class FakeEmailRebindVerificationRepository : IEmailRebindVerificationRepository
    {
        private EmailRebindVerification _verification;

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
            _verification = verification;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    internal sealed class FakeSecretHashService : ISecretHashService
    {
        public string Hash(string value) => $"hashed::{value}";
        public bool Verify(string value, string hash) => hash == $"hashed::{value}";
    }

    internal sealed class FixedCodeGenerator : IRegistrationVerificationCodeGenerator
    {
        public string Generate() => "654321";
    }

    internal sealed class FakeVerificationCodeSender : IRegistrationVerificationCodeSender
    {
        public Task SendAsync(EmailAddress emailAddress, string verificationCode, VerificationCodePurpose purpose, CancellationToken cancellationToken)
            => Task.CompletedTask;
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
