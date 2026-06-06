namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;

public sealed class UserProfileEntity {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public uint SourceColorArgb { get; set; } = 0xFF0078D4;
    public bool IsDarkMode { get; set; }
    public int ContrastLevel { get; set; }
    public byte ThemeVariantCode { get; set; } = 1;

    public string? DisplayName { get; set; }
    public string Gender { get; set; } = "unknown";
    public string? PreferredLanguage { get; set; }
    public string? Metadata { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}