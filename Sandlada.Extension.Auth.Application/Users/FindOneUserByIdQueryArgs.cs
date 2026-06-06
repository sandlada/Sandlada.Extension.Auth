namespace Sandlada.Extension.Auth.Application.Users;

public sealed record FindOneUserByIdQueryArgs {
    public required Guid UserId { get; init; }
}
