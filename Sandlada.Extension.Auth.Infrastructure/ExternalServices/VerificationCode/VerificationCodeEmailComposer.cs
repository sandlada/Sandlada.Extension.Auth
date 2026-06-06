using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.Security;
using MimeKit;
using System.Net;
using System.Text;

namespace Sandlada.Extension.Auth.Infrastructure.ExternalServices.VerificationCode;

public static class VerificationCodeEmailComposer {
    private static readonly string HtmlTemplate = LoadTemplate("VerificationCodeEmail.html");
    private static readonly string TextTemplate = LoadTemplate("VerificationCodeEmail.txt");

    public static MimeMessage BuildMessage(
        EmailAddress emailAddress,
        string verificationCode,
        VerificationCodePurpose purpose,
        SmtpVerificationCodeSenderOptions smtpOptions
    ) {
        var subject = GetSubject(purpose);
        var fromDisplayName = string.IsNullOrWhiteSpace(smtpOptions.FromDisplayName) ? "Sandlada Extension Auth" : smtpOptions.FromDisplayName.Trim();

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromDisplayName, smtpOptions.FromAddress));
        message.To.Add(new MailboxAddress(emailAddress.Value, emailAddress.Value));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder {
            TextBody = BuildTextBody(subject, verificationCode, purpose),
            HtmlBody = BuildHtmlBody(subject, verificationCode, purpose),
        };

        message.Body = bodyBuilder.ToMessageBody();
        return message;
    }

    private static string GetSubject(VerificationCodePurpose purpose) {
        return purpose switch {
            VerificationCodePurpose.EmailRebind => "Sandlada Extension Auth Email Rebind Verification Code",
            _ => "Sandlada Extension Auth Registration Verification Code",
        };
    }

    private static string BuildTextBody(string subject, string verificationCode, VerificationCodePurpose purpose) {
        var template = RenderTemplate(TextTemplate, CreateTemplateValues(subject, verificationCode, purpose));
        return template;
    }

    private static string BuildHtmlBody(string subject, string verificationCode, VerificationCodePurpose purpose) {
        var values = CreateTemplateValues(subject, verificationCode, purpose, htmlEncode: true);
        return RenderTemplate(HtmlTemplate, values);
    }

    private static string GetPurposeLine(VerificationCodePurpose purpose) {
        return purpose switch {
            VerificationCodePurpose.EmailRebind => "You requested to update the email address on your Sandlada Extension Auth account.",
            VerificationCodePurpose.Registration => "You requested a Sandlada Extension Auth registration verification code.",
        };
    }
    private static string GetMainTitle(VerificationCodePurpose purpose) {
        return purpose switch {
            VerificationCodePurpose.EmailRebind => "Sandlada Extension Auth Email Rebind Verification Code",
            VerificationCodePurpose.Registration => "Sandlada Extension Auth Registration Verification Code",
        };
    }
    private static string GetMainDesc(VerificationCodePurpose purpose) {
        return purpose switch {
            VerificationCodePurpose.EmailRebind => "Use the following verification code to update the email address on your Sandlada Extension Auth account.",
            VerificationCodePurpose.Registration => "Use the following verification code to complete your registration.",
        };
    }

    private static IReadOnlyDictionary<string, string> CreateTemplateValues(
        string subject,
        string verificationCode,
        VerificationCodePurpose purpose,
        bool htmlEncode = false
    ) {
        var purposeLine = GetPurposeLine(purpose);
        if (htmlEncode) {
            subject = WebUtility.HtmlEncode(subject);
            verificationCode = WebUtility.HtmlEncode(verificationCode);
            purposeLine = WebUtility.HtmlEncode(purposeLine);
        }

        return new Dictionary<string, string>(StringComparer.Ordinal) {
            ["{{MAIN_TITLE}}"] = GetMainTitle(purpose),
            ["{{MAIN_DESC}}"] = GetMainDesc(purpose),
            ["{{SUBJECT}}"] = subject,
            ["{{PURPOSE}}"] = purposeLine,
            ["{{VERIFICATION_CODE}}"] = verificationCode,
            ["{{EXPIRATION_MINUTES}}"] = "10",
        };
    }

    private static string RenderTemplate(string template, IReadOnlyDictionary<string, string> values) {
        var rendered = template;
        foreach (var entry in values) {
            rendered = rendered.Replace(entry.Key, entry.Value, StringComparison.Ordinal);
        }

        return rendered;
    }

    private static string LoadTemplate(string templateSuffix) {
        var assembly = typeof(VerificationCodeEmailComposer).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(templateSuffix, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null) {
            throw new InvalidOperationException($"Missing embedded email template resource ending with '{templateSuffix}'.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Unable to open embedded email template resource '{resourceName}'.");

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }
}
