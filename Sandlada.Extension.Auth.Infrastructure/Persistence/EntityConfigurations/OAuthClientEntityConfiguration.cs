using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.EntityConfigurations;

public sealed class OAuthClientEntityConfiguration : IEntityTypeConfiguration<OAuthClientEntity> {
    public void Configure(EntityTypeBuilder<OAuthClientEntity> builder) {
        builder.ToTable("OAuthClients");
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.ClientId).IsRequired().HasMaxLength(100);
        builder.HasIndex(entity => entity.ClientId).IsUnique();
        builder.Property(entity => entity.DisplayName).IsRequired().HasMaxLength(200);
        builder.Property(entity => entity.RedirectUris).IsRequired();
        builder.Property(entity => entity.PostLogoutRedirectUris).IsRequired();
        builder.Property(entity => entity.AllowedScopes).IsRequired();
        builder.Property(entity => entity.AllowedGrantTypes).IsRequired();
    }
}