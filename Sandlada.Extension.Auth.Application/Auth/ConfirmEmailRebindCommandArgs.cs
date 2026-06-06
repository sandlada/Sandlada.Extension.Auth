namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record ConfirmEmailRebindCommandArgs {
    public required string EmailAddress { get; init; }
    public required string VerificationCode { get; init; }
}
