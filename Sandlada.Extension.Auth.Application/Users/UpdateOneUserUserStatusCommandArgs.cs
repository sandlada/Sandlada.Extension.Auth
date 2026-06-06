namespace Sandlada.Extension.Auth.Application.Users;

public sealed record UpdateOneUserUserStatusCommandArgs {
    public required string Status { get; init; }
}
