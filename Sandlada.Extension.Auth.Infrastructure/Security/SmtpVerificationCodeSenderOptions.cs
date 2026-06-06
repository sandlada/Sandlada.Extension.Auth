namespace Sandlada.Extension.Auth.Infrastructure.Security;

public sealed record SmtpVerificationCodeSenderOptions {
    public const string SectionName = "Email:Smtp";

    public bool Enabled { get; init; } = true;
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool UseSsl { get; init; } = true;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string FromDisplayName { get; init; } = "Sandlada Extension Auth";
    public int TimeoutSeconds { get; init; } = 15;
}
