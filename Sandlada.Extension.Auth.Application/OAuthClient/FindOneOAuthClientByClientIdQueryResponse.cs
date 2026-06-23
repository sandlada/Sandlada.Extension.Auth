namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record FindOneOAuthClientByClientIdQueryResponse {
    public required Guid Id { get; init; }
    public required string ClientId { get; init; }
    public required string DisplayName { get; init; }
    public required List<string> RedirectUris { get; init; }
    public required List<string> PostLogoutRedirectUris { get; init; }
    public required List<string> AllowedScopes { get; init; }
    public required List<string> AllowedGrantTypes { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static FindOneOAuthClientByClientIdQueryResponse From(Domain.Aggregates.OAuthClient client) {
        return new FindOneOAuthClientByClientIdQueryResponse {
            Id = client.Id,
            ClientId = client.ClientId,
            DisplayName = client.DisplayName,
            RedirectUris = client.RedirectUris,
            PostLogoutRedirectUris = client.PostLogoutRedirectUris,
            AllowedScopes = client.AllowedScopes,
            AllowedGrantTypes = client.AllowedGrantTypes,
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
        };
    }
}