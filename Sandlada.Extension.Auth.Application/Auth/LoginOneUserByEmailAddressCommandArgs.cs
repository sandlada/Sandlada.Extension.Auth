namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByEmailAddressCommandArgs {
    public required string EmailAddress { get; init; }
    public required string Password { get; init; }
}
