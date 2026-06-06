using Sandlada.Extension.Auth.Domain.Aggregates;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed record UserResponse {
    public required Guid UserId { get; init; }
    public required string EmailAddress { get; init; }
    public required string? UniqueName { get; init; }
    public required string Role { get; init; }
    public required bool IsEmailVerified { get; init; }
    public required string Status { get; init; }
    public required DateTime? FirstLoginAt { get; init; }
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }

    public static UserResponse From(User user) {
        return new UserResponse {
            UserId = user.Id,
            EmailAddress = user.EmailAddress.Value,
            UniqueName = user.UniqueName,
            Role = user.Role.Value,
            IsEmailVerified = user.IsEmailVerified,
            Status = user.Status.Code,
            FirstLoginAt = user.FirstLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }
}
