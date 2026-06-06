using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Repositories;

public interface IRegistrationVerificationRepository {
    Task<IResult> InsertOneAsync(RegistrationVerification registrationVerification);

    Task<IResult<RegistrationVerification>> FindOneByEmailAddressAsync(EmailAddress emailAddress);

    Task<IResult> UpdateOneAsync(RegistrationVerification registrationVerification);

    Task<IResult> RemoveOneAsync(Guid id);
}
