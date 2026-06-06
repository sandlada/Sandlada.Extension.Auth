namespace Sandlada.Extension.Auth.Application.Users;

public sealed record RemoveOneUserCommandResponse {
    public required bool Removed { get; init; }
}
