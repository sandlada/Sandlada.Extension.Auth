namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record InsertOneOAuthClientCommandArgs {
    public required string ClientId { get; init; }
    public required string DisplayName { get; init; }
    public required List<string> RedirectUris { get; init; }
    public List<string> PostLogoutRedirectUris { get; init; } = [];
    public List<string> AllowedScopes { get; init; } = [];
    public List<string> AllowedGrantTypes { get; init; } = [];
}