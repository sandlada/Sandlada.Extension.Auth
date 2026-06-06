namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record RemoveOneUserProfileCommandResponse {
    public required bool Removed { get; init; }
}
