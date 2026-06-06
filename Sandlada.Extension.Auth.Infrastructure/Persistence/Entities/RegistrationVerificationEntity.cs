namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;

public sealed class RegistrationVerificationEntity {
    public Guid Id { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string EmailAddressNormalized { get; set; } = string.Empty;
    public string VerificationCodeHash { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int FailedAttemptCount { get; set; }
    public int RequestCount { get; set; }
    public DateTime RequestCountDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ConsumedAt { get; set; }
}
