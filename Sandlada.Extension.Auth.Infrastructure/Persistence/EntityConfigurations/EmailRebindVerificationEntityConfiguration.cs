using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.EntityConfigurations;

public sealed class EmailRebindVerificationEntityConfiguration : IEntityTypeConfiguration<EmailRebindVerificationEntity> {
    public void Configure(EntityTypeBuilder<EmailRebindVerificationEntity> builder) {
        builder.ToTable("EmailRebindVerifications");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.UserId).IsRequired();
        builder.HasIndex(entity => entity.UserId).IsUnique();
        builder.Property(entity => entity.TargetEmailAddress).IsRequired();
        builder.Property(entity => entity.TargetEmailAddressNormalized).IsRequired();
        builder.Property(entity => entity.VerificationCodeHash).IsRequired();
        builder.Property(entity => entity.ExpiresAt).IsRequired();
        builder.Property(entity => entity.FailedAttemptCount).IsRequired().HasDefaultValue(0);
        builder.Property(entity => entity.RequestCount).IsRequired().HasDefaultValue(1);
        builder.Property(entity => entity.RequestCountDate).IsRequired().HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt).IsRequired();
    }
}
