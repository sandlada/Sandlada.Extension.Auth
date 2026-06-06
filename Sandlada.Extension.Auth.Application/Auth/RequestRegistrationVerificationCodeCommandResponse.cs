namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RequestRegistrationVerificationCodeCommandResponse {
    public required string EmailAddress { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
