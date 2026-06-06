using Sandlada.Extension.Auth.Application.Auth;
using Microsoft.AspNetCore.Identity;

namespace Sandlada.Extension.Auth.Infrastructure.Security;

public sealed class SecretHashService : ISecretHashService {
    private readonly PasswordHasher<string> passwordHasher = new();

    public string Hash(string value) {
        return this.passwordHasher.HashPassword(string.Empty, value);
    }

    public bool Verify(string value, string hash) {
        var verificationResult = this.passwordHasher.VerifyHashedPassword(string.Empty, hash, value);
        return verificationResult != PasswordVerificationResult.Failed;
    }
}
