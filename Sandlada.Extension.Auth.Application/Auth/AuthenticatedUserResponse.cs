using Sandlada.Extension.Auth.Domain.Aggregates;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record AuthenticatedUserResponse {
    public required Guid UserId { get; init; }
    public required string EmailAddress { get; init; }
    public required string? UniqueName { get; init; }
    public required string Role { get; init; }
    public required bool IsEmailVerified { get; init; }
    public required DateTime? FirstLoginAt { get; init; }

    public static AuthenticatedUserResponse From(User user) {
        return new AuthenticatedUserResponse {
            UserId = user.Id,
            EmailAddress = user.EmailAddress.Value,
            UniqueName = user.UniqueName,
            Role = user.Role.Value,
            IsEmailVerified = user.IsEmailVerified,
            FirstLoginAt = user.FirstLoginAt,
        };
    }
}
