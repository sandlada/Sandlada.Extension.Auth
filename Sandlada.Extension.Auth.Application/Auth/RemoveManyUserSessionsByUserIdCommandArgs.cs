namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RemoveManyUserSessionsByUserIdCommandArgs {
    public required Guid UserId { get; init; }
}
