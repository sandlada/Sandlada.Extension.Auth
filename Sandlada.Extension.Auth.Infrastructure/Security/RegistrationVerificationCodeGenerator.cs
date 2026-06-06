using Sandlada.Extension.Auth.Application.Auth;
using System.Security.Cryptography;

namespace Sandlada.Extension.Auth.Infrastructure.Security;

public sealed class RegistrationVerificationCodeGenerator : IRegistrationVerificationCodeGenerator {
    public string Generate() {
        var verificationCode = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return verificationCode.ToString("D6");
    }
}
