namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RemoveOneUserSessionCommandArgs {
    public required string SessionId { get; init; }
}
