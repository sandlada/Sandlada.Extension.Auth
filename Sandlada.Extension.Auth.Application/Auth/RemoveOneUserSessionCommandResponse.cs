namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RemoveOneUserSessionCommandResponse {
    public required bool Removed { get; init; }
}
