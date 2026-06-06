namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record FindOneUserProfileByUserIdQueryArgs {
    public required Guid UserId { get; init; }
}
