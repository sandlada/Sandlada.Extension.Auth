namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record RemoveOneOAuthClientCommandArgs {
    public required Guid Id { get; init; }
}