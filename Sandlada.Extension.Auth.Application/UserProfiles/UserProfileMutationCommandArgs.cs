namespace Sandlada.Extension.Auth.Application.UserProfiles;

public abstract record UserProfileMutationCommandArgs {
    public uint? SourceColorArgb { get; init; }
    public bool? IsDarkMode { get; init; }
    public int? ContrastLevel { get; init; }
    public byte? ThemeVariantCode { get; init; }

    public string? DisplayName { get; init; }
    public string? Gender { get; init; }

    public string? PreferredLanguage { get; init; }
    public string? Metadata { get; init; }
}