namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RequestLoginVerificationCodeCommandArgs {
    public string? EmailAddress { get; init; }
    public string? UniqueName { get; init; }
}