using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.EntityConfigurations;

public sealed class UserProfileEntityConfiguration : IEntityTypeConfiguration<UserProfileEntity> {
    public void Configure(EntityTypeBuilder<UserProfileEntity> builder) {
        builder.ToTable("UserProfiles");
        builder.HasKey(entity => new { entity.Id, entity.UserId });
        builder.Property(entity => entity.Id).ValueGeneratedNever();
        builder.Property(entity => entity.UserId).ValueGeneratedNever();
        builder.HasIndex(entity => entity.UserId).IsUnique();

        builder.HasOne<UserEntity>()
            .WithOne()
            .HasForeignKey<UserProfileEntity>(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(entity => entity.SourceColorArgb).IsRequired().HasDefaultValue(0xFF0078D4u);
        builder.Property(entity => entity.IsDarkMode).IsRequired().HasDefaultValue(false);
        builder.Property(entity => entity.ContrastLevel).IsRequired().HasDefaultValue(0);
        builder.Property(entity => entity.ThemeVariantCode).IsRequired().HasDefaultValue((byte)1);

        builder.Property(entity => entity.DisplayName).IsRequired(false);
        builder.Property(entity => entity.Gender).IsRequired().HasDefaultValue("unknown");

        builder.Property(entity => entity.PreferredLanguage).IsRequired(false);
        builder.Property(entity => entity.Metadata).IsRequired(false);

        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt).IsRequired();
    }
}