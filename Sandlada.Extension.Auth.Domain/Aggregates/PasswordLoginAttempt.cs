using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Primitives;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Aggregates;

public sealed record PasswordLoginAttemptConstructorArgs {
    public required Guid Id { get; init; }
    public required EmailAddress EmailAddress { get; init; }
    public required int FailedAttemptCount { get; init; }
    public DateTime? LockoutEnd { get; init; }
    public required int RequestCount { get; init; }
    public required DateTime RequestCountDate { get; init; }
    public DateTime? LastFailedAttemptAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

public sealed class PasswordLoginAttempt : IAggregate<Guid> {

    #region Constants
    private const int MaxFailedAttemptCount = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const int MaxDailyRequestCount = 20;
    #endregion

    #region Properties
    public Guid Id { get; private set; }
    public EmailAddress EmailAddress { get; private set; } = default!;
    public int FailedAttemptCount { get; private set; }
    public DateTime? LockoutEnd { get; private set; }
    public int RequestCount { get; private set; }
    public DateTime RequestCountDate { get; private set; } = DateTime.UnixEpoch;
    public DateTime? LastFailedAttemptAt { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UnixEpoch;
    public DateTime UpdatedAt { get; private set; } = DateTime.UnixEpoch;

    public bool IsLockedOut(DateTime utcNow) => this.LockoutEnd is not null && this.LockoutEnd > utcNow;
    #endregion

    #region Constructors
    private PasswordLoginAttempt() {
    }

    private PasswordLoginAttempt(PasswordLoginAttemptConstructorArgs args) {
        this.Id = args.Id;
        this.EmailAddress = args.EmailAddress;
        this.FailedAttemptCount = args.FailedAttemptCount;
        this.LockoutEnd = args.LockoutEnd;
        this.RequestCount = args.RequestCount;
        this.RequestCountDate = args.RequestCountDate;
        this.LastFailedAttemptAt = args.LastFailedAttemptAt;
        this.CreatedAt = args.CreatedAt;
        this.UpdatedAt = args.UpdatedAt;
    }

    public static IResult<PasswordLoginAttempt> From(PasswordLoginAttemptConstructorArgs args) {
        if (args.FailedAttemptCount < 0) return Result.Failure<PasswordLoginAttempt>(DomainError.Auth.InvalidVerificationChallenge);
        if (args.RequestCount < 0) return Result.Failure<PasswordLoginAttempt>(DomainError.Auth.InvalidVerificationChallenge);
        return Result.Success(new PasswordLoginAttempt(args));
    }

    public static IResult<PasswordLoginAttempt> CreateNew(EmailAddress emailAddress, DateTime utcNow) {
        return Result.Success(new PasswordLoginAttempt(new PasswordLoginAttemptConstructorArgs {
            Id = Guid.NewGuid(),
            EmailAddress = emailAddress,
            FailedAttemptCount = 0,
            LockoutEnd = null,
            RequestCount = 0,
            RequestCountDate = utcNow.Date,
            LastFailedAttemptAt = null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        }));
    }
    #endregion

    #region Methods
    public IResult RegisterRequest(DateTime utcNow) {
        var currentDay = utcNow.Date;
        if (this.RequestCountDate.Date != currentDay) {
            this.RequestCount = 0;
            this.RequestCountDate = currentDay;
        }

        if (this.RequestCount >= MaxDailyRequestCount) {
            return Result.Failure(DomainError.Auth.PasswordLoginRequestLimitExceeded);
        }

        this.RequestCount += 1;
        this.UpdatedAt = utcNow;
        return Result.Success();
    }

    public IResult RegisterFailedAttempt(DateTime utcNow) {
        if (this.IsLockedOut(utcNow)) {
            return Result.Failure(DomainError.Auth.PasswordLoginFailedAttemptLimitExceeded);
        }

        this.FailedAttemptCount += 1;
        this.LastFailedAttemptAt = utcNow;
        this.UpdatedAt = utcNow;

        if (this.FailedAttemptCount >= MaxFailedAttemptCount) {
            this.LockoutEnd = utcNow.Add(LockoutDuration);
            return Result.Failure(DomainError.Auth.PasswordLoginFailedAttemptLimitExceeded);
        }

        return Result.Success();
    }

    public IResult Reset(DateTime utcNow) {
        this.FailedAttemptCount = 0;
        this.LockoutEnd = null;
        this.UpdatedAt = utcNow;
        return Result.Success();
    }
    #endregion
}
