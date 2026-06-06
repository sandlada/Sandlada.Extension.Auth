using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Primitives;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Aggregates;

public sealed record LoginVerificationConstructorArgs {
    public required Guid Id { get; init; }
    public required EmailAddress EmailAddress { get; init; }
    public required string VerificationCodeHash { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public required int FailedAttemptCount { get; init; }
    public required int RequestCount { get; init; }
    public required DateTime RequestCountDate { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
    public DateTime? ConsumedAt { get; init; }
}

public sealed class LoginVerification : IAggregate<Guid> {

    #region Properties
    public Guid Id { get; private set; }
    public EmailAddress EmailAddress { get; private set; } = default!;
    public string VerificationCodeHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; } = DateTime.UnixEpoch;
    public int FailedAttemptCount { get; private set; }
    public int RequestCount { get; private set; }
    public DateTime RequestCountDate { get; private set; } = DateTime.UnixEpoch;
    public DateTime CreatedAt { get; private set; } = DateTime.UnixEpoch;
    public DateTime UpdatedAt { get; private set; } = DateTime.UnixEpoch;
    public DateTime? ConsumedAt { get; private set; }
    public bool IsConsumed => this.ConsumedAt is not null;
    public bool IsFailedAttemptLimitExceeded => this.FailedAttemptCount >= MaxFailedAttemptCount;

    private const int MaxFailedAttemptCount = 5;
    #endregion

    #region Constructors
    private LoginVerification() {
    }

    private LoginVerification(LoginVerificationConstructorArgs args) {
        this.Id = args.Id;
        this.EmailAddress = args.EmailAddress;
        this.VerificationCodeHash = args.VerificationCodeHash;
        this.ExpiresAt = args.ExpiresAt;
        this.FailedAttemptCount = args.FailedAttemptCount;
        this.RequestCount = args.RequestCount;
        this.RequestCountDate = args.RequestCountDate;
        this.CreatedAt = args.CreatedAt;
        this.UpdatedAt = args.UpdatedAt;
        this.ConsumedAt = args.ConsumedAt;
    }

    public static IResult<LoginVerification> From(LoginVerificationConstructorArgs args) {
        if (string.IsNullOrWhiteSpace(args.VerificationCodeHash)) return Result.Failure<LoginVerification>(DomainError.Auth.InvalidVerificationChallenge);
        if (args.ExpiresAt <= args.CreatedAt) return Result.Failure<LoginVerification>(DomainError.Auth.InvalidVerificationChallenge);
        if (args.FailedAttemptCount < 0) return Result.Failure<LoginVerification>(DomainError.Auth.InvalidVerificationChallenge);
        if (args.RequestCount < 0) return Result.Failure<LoginVerification>(DomainError.Auth.InvalidVerificationChallenge);
        return Result.Success(new LoginVerification(args));
    }
    #endregion

    #region Methods
    public bool IsExpired(DateTime utcNow) => utcNow >= this.ExpiresAt;

    public IResult RegisterRequest(DateTime utcNow) {
        var currentDay = utcNow.Date;
        if (this.RequestCountDate.Date != currentDay) {
            this.RequestCount = 0;
            this.RequestCountDate = currentDay;
        }

        if (this.RequestCount >= 10) return Result.Failure(DomainError.Auth.LoginRequestLimitExceeded);

        this.RequestCount += 1;
        this.UpdatedAt = utcNow;
        return Result.Success();
    }

    public IResult Renew(string verificationCodeHash, DateTime expiresAt, DateTime utcNow) {
        if (string.IsNullOrWhiteSpace(verificationCodeHash)) return Result.Failure(DomainError.Auth.InvalidVerificationChallenge);
        if (expiresAt <= utcNow) return Result.Failure(DomainError.Auth.InvalidVerificationChallenge);

        this.VerificationCodeHash = verificationCodeHash;
        this.ExpiresAt = expiresAt;
        this.FailedAttemptCount = 0;
        this.ConsumedAt = null;
        this.UpdatedAt = utcNow;
        return Result.Success();
    }

    public IResult RegisterFailedAttempt(DateTime utcNow) {
        if (this.IsConsumed) return Result.Failure(DomainError.Auth.VerificationCodeAlreadyUsed);
        if (this.IsExpired(utcNow)) return Result.Failure(DomainError.Auth.VerificationCodeExpired);
        if (this.IsFailedAttemptLimitExceeded) return Result.Failure(DomainError.Auth.VerificationCodeAttemptLimitExceeded);

        this.FailedAttemptCount += 1;
        this.UpdatedAt = utcNow;
        if (this.IsFailedAttemptLimitExceeded) {
            return Result.Failure(DomainError.Auth.VerificationCodeAttemptLimitExceeded);
        }

        return Result.Success();
    }

    public IResult Consume(DateTime utcNow) {
        if (this.IsConsumed) return Result.Failure(DomainError.Auth.VerificationCodeAlreadyUsed);
        if (this.IsExpired(utcNow)) return Result.Failure(DomainError.Auth.VerificationCodeExpired);
        if (this.IsFailedAttemptLimitExceeded) return Result.Failure(DomainError.Auth.VerificationCodeAttemptLimitExceeded);

        this.ConsumedAt = utcNow;
        this.UpdatedAt = utcNow;
        return Result.Success();
    }
    #endregion
}