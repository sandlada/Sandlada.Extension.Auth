namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RequestEmailRebindVerificationCodeCommandResponse {
    public required string EmailAddress { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
