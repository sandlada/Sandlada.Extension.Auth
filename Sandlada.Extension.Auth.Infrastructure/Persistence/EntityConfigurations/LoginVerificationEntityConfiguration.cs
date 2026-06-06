using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.EntityConfigurations;

public sealed class LoginVerificationEntityConfiguration : IEntityTypeConfiguration<LoginVerificationEntity> {
    public void Configure(EntityTypeBuilder<LoginVerificationEntity> builder) {
        builder.ToTable("LoginVerifications");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.EmailAddress).IsRequired();
        builder.Property(entity => entity.EmailAddressNormalized).IsRequired();
        builder.HasIndex(entity => entity.EmailAddressNormalized).IsUnique();
        builder.Property(entity => entity.VerificationCodeHash).IsRequired();
        builder.Property(entity => entity.ExpiresAt).IsRequired();
        builder.Property(entity => entity.FailedAttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(entity => entity.RequestCount).IsRequired().HasDefaultValue(1);
        builder.Property(entity => entity.RequestCountDate).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt).IsRequired();
    }
}