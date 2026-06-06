namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByUniqueNameAndPasswordCommandArgs {
    public required string UniqueName { get; init; }
    public required string Password { get; init; }
}