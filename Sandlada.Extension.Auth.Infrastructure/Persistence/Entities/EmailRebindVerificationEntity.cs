namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;

public sealed class EmailRebindVerificationEntity {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TargetEmailAddress { get; set; } = string.Empty;
    public string TargetEmailAddressNormalized { get; set; } = string.Empty;
    public string VerificationCodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int FailedAttemptCount { get; set; }
    public int RequestCount { get; set; }
    public DateTime RequestCountDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
}
