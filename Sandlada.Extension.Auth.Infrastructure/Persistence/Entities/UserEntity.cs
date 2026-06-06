namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;

public sealed class UserEntity {
    public Guid Id { get; set; }
    public string EmailAddress { get; set; } = string.Empty;
    public string EmailAddressNormalized { get; set; } = string.Empty;
    public string? UniqueName { get; set; }
    public string? UniqueNameNormalized { get; set; }
    public string Role { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailVerified { get; set; }
    public string Status { get; set; } = "Enabled";
    public DateTime? FirstLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
