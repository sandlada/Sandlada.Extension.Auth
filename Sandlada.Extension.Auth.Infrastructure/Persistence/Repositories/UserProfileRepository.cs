using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;

public sealed class UserProfileRepository : IUserProfileRepository {
    private readonly AuthDbContext dbContext;

    public UserProfileRepository(AuthDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public Task<IResult> InsertOneAsync(UserProfile userProfile) {
        this.dbContext.UserProfiles.Add(this.ToEntity(userProfile));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult<UserProfile>> FindOneByUserIdAsync(Guid userId) {
        var entity = await this.dbContext.UserProfiles.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId);
        return entity is null
            ? Result.Failure<UserProfile>(DomainError.UserProfile.NotFound)
            : this.ToDomain(entity);
    }

    public Task<IResult> UpdateOneAsync(UserProfile userProfile) {
        this.dbContext.UserProfiles.Update(this.ToEntity(userProfile));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult> RemoveOneByUserIdAsync(Guid userId) {
        var entity = await this.dbContext.UserProfiles.FirstOrDefaultAsync(item => item.UserId == userId);
        if (entity is null) return Result.Failure(DomainError.UserProfile.NotFound);

        this.dbContext.UserProfiles.Remove(entity);
        return Result.Success();
    }

    private IResult<UserProfile> ToDomain(UserProfileEntity entity) {
        var contrastLevelResult = MaterialContrastLevel.From(new MaterialContrastLevelConstructorArgs {
            Level = entity.ContrastLevel,
        });
        if (contrastLevelResult.IsFailure) return Result.Failure<UserProfile>(contrastLevelResult.Error);

        var variantResult = MaterialVariant.From(entity.ThemeVariantCode);
        if (variantResult.IsFailure) return Result.Failure<UserProfile>(variantResult.Error);

        var genderResult = Gender.From(entity.Gender);
        if (genderResult.IsFailure) return Result.Failure<UserProfile>(genderResult.Error);

        return UserProfile.From(new UserProfileConstructorArgs {
            Id = entity.Id,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            SourceColorArgb = entity.SourceColorArgb,
            IsDarkMode = entity.IsDarkMode,
            ContrastLevel = contrastLevelResult.Value,
            Variant = variantResult.Value,
            DisplayName = entity.DisplayName,
            Gender = genderResult.Value,
            PreferredLanguage = entity.PreferredLanguage,
            Metadata = entity.Metadata,
        });
    }

    private UserProfileEntity ToEntity(UserProfile userProfile) {
        return new UserProfileEntity {
            Id = userProfile.Id,
            UserId = userProfile.UserId,
            SourceColorArgb = userProfile.SourceColorArgb,
            IsDarkMode = userProfile.IsDarkMode,
            ContrastLevel = userProfile.ContrastLevel.Level,
            ThemeVariantCode = userProfile.Variant.Code,
            DisplayName = userProfile.DisplayName,
            Gender = userProfile.Gender.Value,
            PreferredLanguage = userProfile.PreferredLanguage,
            Metadata = userProfile.Metadata,
            CreatedAt = userProfile.CreatedAt,
            UpdatedAt = userProfile.UpdatedAt,
        };
    }
}