using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.EntityConfigurations;

public sealed class UserEntityConfiguration : IEntityTypeConfiguration<UserEntity> {
    public void Configure(EntityTypeBuilder<UserEntity> builder) {
        builder.ToTable("Users");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.EmailAddress).IsRequired();
        builder.Property(entity => entity.EmailAddressNormalized).IsRequired();
        builder.HasIndex(entity => entity.EmailAddressNormalized).IsUnique();
        builder.Property(entity => entity.UniqueName).IsRequired(false);
        builder.Property(entity => entity.UniqueNameNormalized).IsRequired(false);
        builder.HasIndex(entity => entity.UniqueNameNormalized).IsUnique().HasFilter("\"UniqueNameNormalized\" IS NOT NULL");
        builder.Property(entity => entity.Role).IsRequired();
        builder.Property(entity => entity.PasswordHash).IsRequired();
        builder.Property(entity => entity.IsEmailVerified).IsRequired().HasDefaultValue(false);
        builder.Property(entity => entity.Status).IsRequired().HasDefaultValue("Enabled");
        builder.Property(entity => entity.FirstLoginAt).IsRequired(false);
    }
}
