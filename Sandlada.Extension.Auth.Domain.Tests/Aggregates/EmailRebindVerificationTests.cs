using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Tests.Aggregates;

public sealed class EmailRebindVerificationTests {
    [Fact]
    public void From_ValidInput_ReturnsSuccess() {
        var result = EmailRebindVerification.From(BuildArgs());

        Assert.True(result.IsSuccess);
        Assert.Equal("new@example.com", result.Value.TargetEmailAddress.Value);
    }

    [Fact]
    public void From_EmptyUserId_ReturnsFailure() {
        var result = EmailRebindVerification.From(BuildArgs(userId: Guid.Empty));

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
    }

    [Fact]
    public void From_EmptyVerificationCodeHash_ReturnsFailure() {
        var result = EmailRebindVerification.From(BuildArgs(verificationCodeHash: "   "));

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
    }

    [Fact]
    public void From_ExpiresAtEqualToCreatedAt_ReturnsFailure() {
        var now = DateTime.UtcNow;
        var result = EmailRebindVerification.From(BuildArgs(createdAt: now, expiresAt: now));

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
    }

    [Fact]
    public void From_NegativeRequestCount_ReturnsFailure() {
        var result = EmailRebindVerification.From(BuildArgs(requestCount: -1));

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
    }

    [Fact]
    public void From_NegativeFailedAttemptCount_ReturnsFailure() {
        var result = EmailRebindVerification.From(BuildArgs(failedAttemptCount: -1));

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
    }

    [Fact]
    public void IsExpired_BoundaryChecks_WorkAsExpected() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(expiresAt: now.AddMinutes(5));

        Assert.False(verification.IsExpired(now.AddMinutes(4)));
        Assert.True(verification.IsExpired(now.AddMinutes(5)));
        Assert.True(verification.IsExpired(now.AddMinutes(6)));
    }

    [Fact]
    public void RegisterRequest_SameDay_BelowLimitIncrementsCountAndUpdatedAt() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(requestCount: 9, requestCountDate: now.Date, updatedAt: now.AddMinutes(-1));

        var result = verification.RegisterRequest(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, verification.RequestCount);
        Assert.Equal(now.Date, verification.RequestCountDate.Date);
        Assert.Equal(now, verification.UpdatedAt);
    }

    [Fact]
    public void RegisterRequest_SameDayAtLimit_ReturnsFailureWithoutMutation() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(requestCount: 10, requestCountDate: now.Date, updatedAt: now.AddMinutes(-1));
        var previousUpdatedAt = verification.UpdatedAt;

        var result = verification.RegisterRequest(now);

        AssertFailure(result, DomainError.Auth.EmailRebindRequestLimitExceeded.Code);
        Assert.Equal(10, verification.RequestCount);
        Assert.Equal(now.Date, verification.RequestCountDate.Date);
        Assert.Equal(previousUpdatedAt, verification.UpdatedAt);
    }

    [Fact]
    public void RegisterRequest_NextDay_ResetsThenIncrementsCount() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(requestCount: 10, requestCountDate: now.AddDays(-1).Date, updatedAt: now.AddDays(-1));

        var result = verification.RegisterRequest(now);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, verification.RequestCount);
        Assert.Equal(now.Date, verification.RequestCountDate.Date);
        Assert.Equal(now, verification.UpdatedAt);
    }

    [Fact]
    public void Renew_EmptyVerificationCodeHash_ReturnsFailureWithoutMutation() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification();
        var originalTargetEmail = verification.TargetEmailAddress;
        var originalHash = verification.VerificationCodeHash;
        var originalExpiresAt = verification.ExpiresAt;
        var originalUpdatedAt = verification.UpdatedAt;

        var result = verification.Renew(EmailAddress.From("another@example.com").Value, " ", now.AddMinutes(10), now);

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
        Assert.Equal(originalTargetEmail, verification.TargetEmailAddress);
        Assert.Equal(originalHash, verification.VerificationCodeHash);
        Assert.Equal(originalExpiresAt, verification.ExpiresAt);
        Assert.Equal(originalUpdatedAt, verification.UpdatedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Renew_ExpiresAtNotAfterUtcNow_ReturnsFailure(int minuteOffset) {
        var now = DateTime.UtcNow;
        var verification = CreateVerification();

        var result = verification.Renew(EmailAddress.From("next@example.com").Value, "new-hash", now.AddMinutes(minuteOffset), now);

        AssertFailure(result, DomainError.Auth.InvalidVerificationChallenge.Code);
    }

    [Fact]
    public void Renew_ConsumedVerification_ResetsConsumedStateAndTarget() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(expiresAt: now.AddMinutes(10), failedAttemptCount: 4);
        verification.Consume(now.AddMinutes(1));
        var updatedTargetEmailAddress = EmailAddress.From("another@example.com").Value;

        var renewResult = verification.Renew(updatedTargetEmailAddress, "new-hash-value", now.AddMinutes(20), now.AddMinutes(2));

        Assert.True(renewResult.IsSuccess);
        Assert.False(verification.IsConsumed);
        Assert.Equal(0, verification.FailedAttemptCount);
        Assert.Equal(updatedTargetEmailAddress, verification.TargetEmailAddress);
        Assert.Equal("new-hash-value", verification.VerificationCodeHash);
        Assert.Equal(now.AddMinutes(20), verification.ExpiresAt);
        Assert.Equal(now.AddMinutes(2), verification.UpdatedAt);
    }

    [Fact]
    public void RegisterFailedAttempt_BelowLimit_IncrementsCountAndKeepsChallengeActive() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(expiresAt: now.AddMinutes(10), failedAttemptCount: 1);

        var result = verification.RegisterFailedAttempt(now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.Equal(2, verification.FailedAttemptCount);
        Assert.False(verification.IsFailedAttemptLimitExceeded);
        Assert.Equal(now.AddMinutes(1), verification.UpdatedAt);
    }

    [Fact]
    public void RegisterFailedAttempt_ReachingLimit_ReturnsAttemptLimitExceeded() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(expiresAt: now.AddMinutes(10), failedAttemptCount: 4);

        var result = verification.RegisterFailedAttempt(now.AddMinutes(1));

        AssertFailure(result, DomainError.Auth.VerificationCodeAttemptLimitExceeded.Code);
        Assert.Equal(5, verification.FailedAttemptCount);
        Assert.True(verification.IsFailedAttemptLimitExceeded);
    }

    [Fact]
    public void Consume_AttemptLimitExceeded_ReturnsFailure() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(expiresAt: now.AddMinutes(10), failedAttemptCount: 5);

        var result = verification.Consume(now.AddMinutes(1));

        AssertFailure(result, DomainError.Auth.VerificationCodeAttemptLimitExceeded.Code);
        Assert.False(verification.IsConsumed);
    }

    [Fact]
    public void Consume_ExpiredVerification_ReturnsFailure() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(expiresAt: now.AddMinutes(1));

        var result = verification.Consume(now.AddMinutes(1));

        AssertFailure(result, DomainError.Auth.VerificationCodeExpired.Code);
        Assert.False(verification.IsConsumed);
    }

    [Fact]
    public void Consume_ActiveVerification_MarksAsConsumed() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(expiresAt: now.AddMinutes(10));

        var result = verification.Consume(now.AddMinutes(1));

        Assert.True(result.IsSuccess);
        Assert.True(verification.IsConsumed);
        Assert.Equal(now.AddMinutes(1), verification.ConsumedAt);
        Assert.Equal(now.AddMinutes(1), verification.UpdatedAt);
    }

    [Fact]
    public void Consume_AlreadyConsumed_ReturnsFailure() {
        var now = DateTime.UtcNow;
        var verification = CreateVerification(expiresAt: now.AddMinutes(10));
        verification.Consume(now.AddMinutes(1));
        var firstConsumedAt = verification.ConsumedAt;

        var result = verification.Consume(now.AddMinutes(2));

        AssertFailure(result, DomainError.Auth.VerificationCodeAlreadyUsed.Code);
        Assert.Equal(firstConsumedAt, verification.ConsumedAt);
    }

    private static EmailRebindVerification CreateVerification(
        Guid? userId = null,
        string verificationCodeHash = "hash-value",
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? expiresAt = null,
        int failedAttemptCount = 0,
        int requestCount = 0,
        DateTime? requestCountDate = null
    ) {
        var result = EmailRebindVerification.From(BuildArgs(userId, verificationCodeHash, createdAt, updatedAt, expiresAt, failedAttemptCount, requestCount, requestCountDate));
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static EmailRebindVerificationConstructorArgs BuildArgs(
        Guid? userId = null,
        string verificationCodeHash = "hash-value",
        DateTime? createdAt = null,
        DateTime? updatedAt = null,
        DateTime? expiresAt = null,
        int failedAttemptCount = 0,
        int requestCount = 0,
        DateTime? requestCountDate = null
    ) {
        var created = createdAt ?? DateTime.UtcNow;
        return new EmailRebindVerificationConstructorArgs {
            Id = Guid.NewGuid(),
            UserId = userId ?? Guid.NewGuid(),
            TargetEmailAddress = EmailAddress.From("new@example.com").Value,
            VerificationCodeHash = verificationCodeHash,
            ExpiresAt = expiresAt ?? created.AddMinutes(10),
            FailedAttemptCount = failedAttemptCount,
            RequestCount = requestCount,
            RequestCountDate = requestCountDate ?? created.Date,
            CreatedAt = created,
            UpdatedAt = updatedAt ?? created,
            ConsumedAt = null,
        };
    }

    private static void AssertFailure(IResult result, string expectedCode) {
        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
    }
}
