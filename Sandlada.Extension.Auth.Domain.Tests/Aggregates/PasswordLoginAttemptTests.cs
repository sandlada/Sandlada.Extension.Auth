using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Tests.Aggregates;

public sealed class PasswordLoginAttemptTests {
    [Fact]
    public void From_ValidInput_ReturnsSuccess() {
        var result = PasswordLoginAttempt.From(BuildArgs());

        Assert.True(result.IsSuccess);
        Assert.Equal("user@example.com", result.Value.EmailAddress.Value);
    }

    [Fact]
    public void From_NegativeFailedAttemptCount_ReturnsFailure() {
        var result = PasswordLoginAttempt.From(BuildArgs(failedAttemptCount: -1));

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
    }

    [Fact]
    public void From_NegativeRequestCount_ReturnsFailure() {
        var result = PasswordLoginAttempt.From(BuildArgs(requestCount: -1));

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
    }

    [Fact]
    public void CreateNew_ValidInput_ReturnsSuccess() {
        var now = DateTime.UtcNow;
        var emailAddress = EmailAddress.From("new@example.com").Value;

        var result = PasswordLoginAttempt.CreateNew(emailAddress, now);

        Assert.True(result.IsSuccess);
        Assert.Equal("new@example.com", result.Value.EmailAddress.Value);
        Assert.Equal(0, result.Value.FailedAttemptCount);
        Assert.Equal(0, result.Value.RequestCount);
        Assert.Equal(now.Date, result.Value.RequestCountDate.Date);
        Assert.Null(result.Value.LockoutEnd);
        Assert.Equal(now, result.Value.CreatedAt);
        Assert.Equal(now, result.Value.UpdatedAt);
    }

    [Fact]
    public void IsLockedOut_NullLockoutEnd_ReturnsFalse() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(lockoutEnd: null);

        Assert.False(attempt.IsLockedOut(now));
    }

    [Fact]
    public void IsLockedOut_LockoutEndInPast_ReturnsFalse() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(lockoutEnd: now.AddMinutes(-1));

        Assert.False(attempt.IsLockedOut(now));
    }

    [Fact]
    public void IsLockedOut_LockoutEndInFuture_ReturnsTrue() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(lockoutEnd: now.AddMinutes(5));

        Assert.True(attempt.IsLockedOut(now));
    }

    [Fact]
    public void RegisterRequest_SameDay_BelowLimitIncrementsCountAndUpdatedAt() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(requestCount: 19, requestCountDate: now.Date, updatedAt: now.AddMinutes(-1));

        var result = attempt.RegisterRequest(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(20, attempt.RequestCount);
        Assert.Equal(now.Date, attempt.RequestCountDate.Date);
        Assert.Equal(now, attempt.UpdatedAt);
    }

    [Fact]
    public void RegisterRequest_SameDay_AtLimitReturnsFailureWithoutMutation() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(requestCount: 20, requestCountDate: now.Date, updatedAt: now.AddMinutes(-2));
        var previousUpdatedAt = attempt.UpdatedAt;

        var result = attempt.RegisterRequest(now);

        AssertFailure(result, DomainError.Auth.PasswordLoginRequestLimitExceeded.Code);
        Assert.Equal(20, attempt.RequestCount);
        Assert.Equal(now.Date, attempt.RequestCountDate.Date);
        Assert.Equal(previousUpdatedAt, attempt.UpdatedAt);
    }

    [Fact]
    public void RegisterRequest_NextDay_ResetsThenIncrementsCount() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(requestCount: 20, requestCountDate: now.AddDays(-1).Date, updatedAt: now.AddDays(-1));

        var result = attempt.RegisterRequest(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, attempt.RequestCount);
        Assert.Equal(now.Date, attempt.RequestCountDate.Date);
        Assert.Equal(now, attempt.UpdatedAt);
    }

    [Fact]
    public void RegisterFailedAttempt_BelowLimit_IncrementsCountAndUpdatesTimestamp() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(failedAttemptCount: 3, updatedAt: now.AddMinutes(-1));

        var result = attempt.RegisterFailedAttempt(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(4, attempt.FailedAttemptCount);
        Assert.Equal(now, attempt.LastFailedAttemptAt);
        Assert.Equal(now, attempt.UpdatedAt);
        Assert.Null(attempt.LockoutEnd);
    }

    [Fact]
    public void RegisterFailedAttempt_ReachingLimit_SetsLockoutAndReturnsFailure() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(failedAttemptCount: 4, updatedAt: now.AddMinutes(-1));

        var result = attempt.RegisterFailedAttempt(now);

        AssertFailure(result, DomainError.Auth.PasswordLoginFailedAttemptLimitExceeded.Code);
        Assert.Equal(5, attempt.FailedAttemptCount);
        Assert.NotNull(attempt.LockoutEnd);
        Assert.Equal(now.AddMinutes(15), attempt.LockoutEnd);
        Assert.Equal(now, attempt.LastFailedAttemptAt);
    }

    [Fact]
    public void RegisterFailedAttempt_WhileLocked_ReturnsFailure() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(failedAttemptCount: 5, lockoutEnd: now.AddMinutes(10), updatedAt: now.AddMinutes(-1));
        var previousUpdatedAt = attempt.UpdatedAt;

        var result = attempt.RegisterFailedAttempt(now);

        AssertFailure(result, DomainError.Auth.PasswordLoginFailedAttemptLimitExceeded.Code);
        Assert.Equal(5, attempt.FailedAttemptCount);
        Assert.Equal(previousUpdatedAt, attempt.UpdatedAt);
    }

    [Fact]
    public void Reset_ClearsFailedCountAndLockout() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(failedAttemptCount: 5, lockoutEnd: now.AddMinutes(10), updatedAt: now.AddMinutes(-1));

        var result = attempt.Reset(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, attempt.FailedAttemptCount);
        Assert.Null(attempt.LockoutEnd);
        Assert.Equal(now, attempt.UpdatedAt);
    }

    [Fact]
    public void RegisterFailedAttempt_AfterReset_WorksAgain() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(failedAttemptCount: 5, lockoutEnd: now.AddMinutes(10), updatedAt: now.AddMinutes(-1));
        attempt.Reset(now.AddMinutes(1));

        var result = attempt.RegisterFailedAttempt(now.AddMinutes(2));

        Assert.True(result.IsSuccess);
        Assert.Equal(1, attempt.FailedAttemptCount);
        Assert.Null(attempt.LockoutEnd);
    }

    [Fact]
    public void RegisterRequest_AfterReset_PreservesRequestCount() {
        var now = DateTime.UtcNow;
        var attempt = CreateAttempt(requestCount: 10, requestCountDate: now.Date, updatedAt: now.AddMinutes(-1));
        attempt.Reset(now);

        var result = attempt.RegisterRequest(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(11, attempt.RequestCount);
    }

    private static PasswordLoginAttempt CreateAttempt(
        int failedAttemptCount = 0,
        int requestCount = 0,
        DateTime? requestCountDate = null,
        DateTime? lockoutEnd = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null
    ) {
        var created = createdAt ?? DateTime.UtcNow;
        var updated = updatedAt ?? created;
        var result = PasswordLoginAttempt.From(BuildArgs(
            failedAttemptCount: failedAttemptCount,
            requestCount: requestCount,
            requestCountDate: requestCountDate ?? created.Date,
            lockoutEnd: lockoutEnd,
            createdAt: created,
            updatedAt: updated
        ));
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static PasswordLoginAttemptConstructorArgs BuildArgs(
        int failedAttemptCount = 0,
        int requestCount = 0,
        DateTime? requestCountDate = null,
        DateTime? lockoutEnd = null,
        DateTime? createdAt = null,
        DateTime? updatedAt = null
    ) {
        var created = createdAt ?? DateTime.UtcNow;
        return new PasswordLoginAttemptConstructorArgs {
            Id = Guid.NewGuid(),
            EmailAddress = EmailAddress.From("user@example.com").Value,
            FailedAttemptCount = failedAttemptCount,
            LockoutEnd = lockoutEnd,
            RequestCount = requestCount,
            RequestCountDate = requestCountDate ?? created.Date,
            LastFailedAttemptAt = null,
            CreatedAt = created,
            UpdatedAt = updatedAt ?? created,
        };
    }

    private static void AssertFailure(IResult result, string expectedCode) {
        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
    }
}
