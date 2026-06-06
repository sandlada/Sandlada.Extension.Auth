namespace Sandlada.Extension.Auth.Application.Users;

public sealed record UserStatusResponse {
    public required string Status { get; init; }
}
