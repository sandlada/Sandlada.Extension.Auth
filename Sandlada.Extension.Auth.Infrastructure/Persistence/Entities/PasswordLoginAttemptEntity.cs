namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;

public sealed class PasswordLoginAttemptEntity {
    public Guid Id { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string EmailAddressNormalized { get; set; } = string.Empty;
    public int FailedAttemptCount { get; set; }
    public DateTime? LockoutEnd { get; set; }
    public int RequestCount { get; set; }
    public DateTime RequestCountDate { get; set; }
    public DateTime? LastFailedAttemptAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
