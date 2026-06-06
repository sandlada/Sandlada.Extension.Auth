using Sandlada.Extension.Auth.Domain.Aggregates;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record UserProfileResponse {
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public required uint SourceColorArgb { get; init; }
    public required bool IsDarkMode { get; init; }
    public required int ContrastLevel { get; init; }
    public required byte ThemeVariantCode { get; init; }

    public required string? displayName { get; init; }
    public required string gender { get; init; }

    public required string? PreferredLanguage { get; init; }
    public required string? Metadata { get; init; }

    public static UserProfileResponse From(UserProfile UserProfile) {
        return new UserProfileResponse {
            Id = UserProfile.Id,
            UserId = UserProfile.UserId,
            CreatedAt = UserProfile.CreatedAt,
            UpdatedAt = UserProfile.UpdatedAt,
            SourceColorArgb = UserProfile.SourceColorArgb,
            IsDarkMode = UserProfile.IsDarkMode,
            ContrastLevel = UserProfile.ContrastLevel.Level,
            ThemeVariantCode = UserProfile.Variant.Code,
            displayName = UserProfile.DisplayName,
            gender = UserProfile.Gender.Value,
            PreferredLanguage = UserProfile.PreferredLanguage,
            Metadata = UserProfile.Metadata,
        };
    }
}