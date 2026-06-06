namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByUniqueNameAndVerificationCodeCommandArgs {
    public required string UniqueName { get; init; }
    public required string VerificationCode { get; init; }
}