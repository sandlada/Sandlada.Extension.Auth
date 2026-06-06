namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByUniqueNameCommandArgs {
    public required string UniqueName { get; init; }
    public required string Password { get; init; }
}
