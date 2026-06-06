namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RegisterOneUserCommandArgs {
    public required string EmailAddress { get; init; }
    public required string VerificationCode { get; init; }
    public required string UniqueName { get; init; }
    public required string Password { get; init; }
}
