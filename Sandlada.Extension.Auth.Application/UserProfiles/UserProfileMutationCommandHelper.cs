using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

internal static class UserProfileMutationCommandHelper {
    public static IResult<UserProfile> CreateOne(Guid userId, UserProfileMutationCommandArgs args) {
        var contrastLevelResult = ResolveContrastLevel(args.ContrastLevel);
        if (contrastLevelResult.IsFailure) return Result.Failure<UserProfile>(contrastLevelResult.Error);

        var themeVariantResult = ResolveThemeVariant(args.ThemeVariantCode);
        if (themeVariantResult.IsFailure) return Result.Failure<UserProfile>(themeVariantResult.Error);

        var genderResult = ResolveGender(args.Gender);
        if (genderResult.IsFailure) return Result.Failure<UserProfile>(genderResult.Error);

        var now = DateTime.UtcNow;

        return UserProfile.From(new UserProfileConstructorArgs {
            Id = Guid.NewGuid(),
            UserId = userId,
            CreatedAt = now,
            UpdatedAt = now,
            SourceColorArgb = args.SourceColorArgb ?? 0xFF0078D4u,
            IsDarkMode = args.IsDarkMode ?? false,
            ContrastLevel = contrastLevelResult.Value,
            Variant = themeVariantResult.Value,
            DisplayName = args.DisplayName,
            Gender = genderResult.Value,
            PreferredLanguage = args.PreferredLanguage,
            Metadata = args.Metadata,
        });
    }

    public static IResult ApplyUpdates(UserProfile UserProfile, UserProfileMutationCommandArgs args) {
        if (args.SourceColorArgb.HasValue) {
            var updateResult = UserProfile.UpdateSourceColorArgb(args.SourceColorArgb.Value);
            if (updateResult.IsFailure) return updateResult;
        }

        if (args.IsDarkMode.HasValue) {
            var updateResult = UserProfile.UpdateIsDarkMode(args.IsDarkMode.Value);
            if (updateResult.IsFailure) return updateResult;
        }

        if (args.ContrastLevel.HasValue) {
            var contrastLevelResult = MaterialContrastLevel.From(new MaterialContrastLevelConstructorArgs {
                Level = args.ContrastLevel.Value,
            });
            if (contrastLevelResult.IsFailure) return contrastLevelResult;

            var updateResult = UserProfile.UpdateContrastLevel(contrastLevelResult.Value);
            if (updateResult.IsFailure) return updateResult;
        }

        if (args.ThemeVariantCode.HasValue) {
            var themeVariantResult = MaterialVariant.From(args.ThemeVariantCode.Value);
            if (themeVariantResult.IsFailure) return themeVariantResult;

            var updateResult = UserProfile.UpdateVariant(themeVariantResult.Value);
            if (updateResult.IsFailure) return updateResult;
        }

        if (args.DisplayName is not null) {
            var updateResult = UserProfile.UpdateDisplayName(args.DisplayName);
            if (updateResult.IsFailure) return updateResult;
        }

        if (args.Gender is not null) {
            var genderResult = Gender.From(args.Gender);
            if (genderResult.IsFailure) return genderResult;

            var updateResult = UserProfile.UpdateGender(genderResult.Value);
            if (updateResult.IsFailure) return updateResult;
        }

        if (args.PreferredLanguage is not null) {
            var updateResult = UserProfile.UpdatePreferredLanguage(args.PreferredLanguage);
            if (updateResult.IsFailure) return updateResult;
        }

        if (args.Metadata is not null) {
            var updateResult = UserProfile.UpdateMetadata(args.Metadata);
            if (updateResult.IsFailure) return updateResult;
        }

        return Result.Success();
    }

    private static IResult<MaterialContrastLevel> ResolveContrastLevel(int? contrastLevel) {
        if (!contrastLevel.HasValue) return Result.Success(MaterialContrastLevel.Default);

        return MaterialContrastLevel.From(new MaterialContrastLevelConstructorArgs {
            Level = contrastLevel.Value,
        });
    }

    private static IResult<MaterialVariant> ResolveThemeVariant(byte? themeVariantCode) {
        if (!themeVariantCode.HasValue) return Result.Success(MaterialVariant.Neutral);

        return MaterialVariant.From(themeVariantCode.Value);
    }

    private static IResult<Gender> ResolveGender(string? gender) {
        if (gender is null) return Result.Success(Gender.Unknown);

        return Gender.From(gender);
    }
}