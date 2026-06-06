using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.ExternalServices.VerificationCode;
using Sandlada.Extension.Auth.Infrastructure.Security;
using MimeKit;

namespace Sandlada.Extension.Auth.Infrastructure.Tests.Email;

public sealed class VerificationCodeEmailComposerTests {
    [Fact]
    public void BuildMessage_Registration_ReturnsMultipartAlternativeWithHtmlAndText() {
        var message = VerificationCodeEmailComposer.BuildMessage(
            EmailAddress.From("user@example.com").Value,
            "123456",
            VerificationCodePurpose.Registration,
            BuildOptions()
        );

        Assert.Equal("Sandlada Extension Auth Registration Verification Code", message.Subject);

        var multipart = Assert.IsType<MultipartAlternative>(message.Body);
        Assert.Equal(2, multipart.Count);

        var textPart = Assert.IsType<TextPart>(multipart[0]);
        var htmlPart = Assert.IsType<TextPart>(multipart[1]);

        Assert.Equal("plain", textPart.ContentType.MediaSubtype);
        Assert.Equal("html", htmlPart.ContentType.MediaSubtype);
        Assert.Contains("VERIFICATION CODE", textPart.Text);
        Assert.Contains("Hello,", textPart.Text);
        Assert.Contains("registration verification code", textPart.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("123456", textPart.Text);
        Assert.Contains("--md-sys-color-surface", htmlPart.Text);
        Assert.Contains("Your Code", htmlPart.Text);
        Assert.Contains("You requested a Sandlada Extension Auth registration verification code.", htmlPart.Text);
    }

    [Fact]
    public void BuildMessage_EmailRebind_UsesPurposeSpecificCopy() {
        var message = VerificationCodeEmailComposer.BuildMessage(
            EmailAddress.From("user@example.com").Value,
            "654321",
            VerificationCodePurpose.EmailRebind,
            BuildOptions(fromDisplayName: "  Sandlada Extension Auth  ")
        );

        Assert.Equal("Sandlada Extension Auth Email Rebind Verification Code", message.Subject);

        var multipart = Assert.IsType<MultipartAlternative>(message.Body);
        var textPart = Assert.IsType<TextPart>(multipart[0]);
        var htmlPart = Assert.IsType<TextPart>(multipart[1]);

        Assert.Contains("email address on your Sandlada Extension Auth account", textPart.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("update the email address on your Sandlada Extension Auth account", htmlPart.Text, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("654321", textPart.Text);
        Assert.Contains("654321", htmlPart.Text);
    }

    [Fact]
    public void BuildMessage_HtmlEncodesVerificationCode() {
        var message = VerificationCodeEmailComposer.BuildMessage(
            EmailAddress.From("user@example.com").Value,
            "<123>&\"456\"",
            VerificationCodePurpose.Registration,
            BuildOptions()
        );

        var multipart = Assert.IsType<MultipartAlternative>(message.Body);
        var htmlPart = Assert.IsType<TextPart>(multipart[1]);

        Assert.Contains("&lt;123&gt;&amp;&quot;456&quot;", htmlPart.Text);
        Assert.DoesNotContain("<123>&\"456\"", htmlPart.Text);
    }

    private static SmtpVerificationCodeSenderOptions BuildOptions(string fromDisplayName = "Sandlada Extension Auth") {
        return new SmtpVerificationCodeSenderOptions {
            Host = "localhost",
            Port = 25,
            FromAddress = "noreply@localhost",
            FromDisplayName = fromDisplayName,
        };
    }
}
