using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Infrastructure.Security;

public sealed class NoopRegistrationVerificationCodeSender : IRegistrationVerificationCodeSender {
    public Task SendAsync(EmailAddress emailAddress, string verificationCode, VerificationCodePurpose purpose, CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
