using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Auth;

public interface ISecretHashService {
    string Hash(string value);
    bool Verify(string value, string hash);
}

public interface IRegistrationVerificationCodeGenerator {
    string Generate();
}

public enum VerificationCodePurpose {
    Registration,
    EmailRebind,
    Login,
}

public interface IRegistrationVerificationCodeSender {
    Task SendAsync(EmailAddress emailAddress, string verificationCode, VerificationCodePurpose purpose, CancellationToken cancellationToken);
}

public interface IApplicationUnitOfWork {
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IAuthSessionRepository {
    Task<IResult<int>> RemoveOneBySessionIdAsync(string sessionId);
    Task<IResult<int>> RemoveManyByUserIdAsync(Guid userId);
}
