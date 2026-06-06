using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.EntityConfigurations;

public sealed class AuthSessionEntityConfiguration : IEntityTypeConfiguration<AuthSessionEntity> {
    public void Configure(EntityTypeBuilder<AuthSessionEntity> builder) {
        builder.ToTable("AuthSessions");
        builder.HasKey(entity => entity.SessionId);
        builder.Property(entity => entity.SessionId).IsRequired();
        builder.Property(entity => entity.UserId).IsRequired();
        builder.HasIndex(entity => entity.UserId);
        builder.Property(entity => entity.TicketData).IsRequired();
        builder.Property(entity => entity.CreatedAt).IsRequired();
        builder.Property(entity => entity.UpdatedAt).IsRequired();
    }
}
