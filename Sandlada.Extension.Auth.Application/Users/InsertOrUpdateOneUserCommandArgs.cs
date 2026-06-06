namespace Sandlada.Extension.Auth.Application.Users;

public sealed record InsertOrUpdateOneUserCommandArgs {
    public Guid? UserId { get; init; }
    public string? EmailAddress { get; init; }
    public string? UniqueName { get; init; }
    public string? Role { get; init; }
    public string? Password { get; init; }
    public bool? IsEmailVerified { get; init; }
    public string? Status { get; init; }
}
