namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record InsertOneOAuthClientCommandResponse {
    public required Guid Id { get; init; }
    public required string ClientId { get; init; }
    public required string DisplayName { get; init; }
    public required string ClientSecret { get; init; }

    public static InsertOneOAuthClientCommandResponse From(Domain.Aggregates.OAuthClient client, string clientSecret) {
        return new InsertOneOAuthClientCommandResponse {
            Id = client.Id,
            ClientId = client.ClientId,
            DisplayName = client.DisplayName,
            ClientSecret = clientSecret,
        };
    }
}