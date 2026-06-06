namespace Sandlada.Extension.Auth.Application.Users;

public sealed record RemoveOneUserCommandArgs {
    public required Guid UserId { get; init; }
}
