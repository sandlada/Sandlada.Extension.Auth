namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record RemoveOneUserProfileCommandArgs {
    public required Guid UserId { get; init; }
}
