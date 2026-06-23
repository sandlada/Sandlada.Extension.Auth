namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;

public sealed class OAuthClientEntity {
    public Guid Id { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string RedirectUris { get; set; } = string.Empty;
    public string PostLogoutRedirectUris { get; set; } = string.Empty;
    public string AllowedScopes { get; set; } = string.Empty;
    public string AllowedGrantTypes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}