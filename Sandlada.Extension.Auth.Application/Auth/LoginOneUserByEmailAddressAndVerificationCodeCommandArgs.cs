namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByEmailAddressAndVerificationCodeCommandArgs {
    public required string EmailAddress { get; init; }
    public required string VerificationCode { get; init; }
}