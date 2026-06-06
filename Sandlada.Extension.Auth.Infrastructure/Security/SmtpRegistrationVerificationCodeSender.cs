using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.ExternalServices.VerificationCode;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sandlada.Extension.Auth.Infrastructure.Security;

public sealed class SmtpRegistrationVerificationCodeSender(
    IOptions<SmtpVerificationCodeSenderOptions> options,
    ILogger<SmtpRegistrationVerificationCodeSender> logger
) : IRegistrationVerificationCodeSender {

    public async Task SendAsync(EmailAddress emailAddress, string verificationCode, VerificationCodePurpose purpose, CancellationToken cancellationToken) {
        var smtpOptions = options.Value;
        if (!smtpOptions.Enabled) {
            logger.LogInformation("SMTP sender is disabled. Skip sending verification code for {EmailAddress}.", emailAddress.Value);
            return;
        }

        if (!IsConfigured(smtpOptions)) {
            throw new InvalidOperationException("Email SMTP settings are incomplete. Please configure Email:Smtp:Host, Port, and FromAddress.");
        }

        using var smtpClient = new SmtpClient();
        smtpClient.Timeout = Math.Max(1, smtpOptions.TimeoutSeconds) * 1000;

        var message = VerificationCodeEmailComposer.BuildMessage(emailAddress, verificationCode, purpose, smtpOptions);

        await smtpClient.ConnectAsync(smtpOptions.Host, smtpOptions.Port, smtpOptions.UseSsl, cancellationToken);
        if (!string.IsNullOrWhiteSpace(smtpOptions.UserName)) {
            await smtpClient.AuthenticateAsync(smtpOptions.UserName, smtpOptions.Password, cancellationToken);
        }

        await smtpClient.SendAsync(message, cancellationToken);
        await smtpClient.DisconnectAsync(true, cancellationToken);

        logger.LogInformation("Sent {Purpose} verification code email to {EmailAddress}.", purpose, emailAddress.Value);
    }

    public static bool IsConfigured(SmtpVerificationCodeSenderOptions options) {
        return !string.IsNullOrWhiteSpace(options.Host)
            && options.Port > 0
            && !string.IsNullOrWhiteSpace(options.FromAddress);
    }
}
