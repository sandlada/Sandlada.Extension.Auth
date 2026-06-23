namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record FindOneOAuthClientByClientIdQueryArgs {
    public required string ClientId { get; init; }
}