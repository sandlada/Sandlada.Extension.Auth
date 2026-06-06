namespace Sandlada.Extension.Auth.Application.Users;

public sealed record UpdateOneUserEmailVerifiedCommandArgs {
    public required Guid UserId { get; init; }
    public required bool IsEmailVerified { get; init; }
}
