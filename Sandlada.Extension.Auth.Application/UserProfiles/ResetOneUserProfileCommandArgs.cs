namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record ResetOneUserProfileCommandArgs {
    public required Guid UserId { get; init; }
}
