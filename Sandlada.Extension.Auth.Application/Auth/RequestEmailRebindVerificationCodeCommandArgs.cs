namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RequestEmailRebindVerificationCodeCommandArgs {
    public required string EmailAddress { get; init; }
}
