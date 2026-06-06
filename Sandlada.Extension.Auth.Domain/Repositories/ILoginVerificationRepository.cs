using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Repositories;

public interface ILoginVerificationRepository {
    Task<IResult> InsertOneAsync(LoginVerification loginVerification);

    Task<IResult<LoginVerification>> FindOneByEmailAddressAsync(EmailAddress emailAddress);

    Task<IResult> UpdateOneAsync(LoginVerification loginVerification);

    Task<IResult> RemoveOneAsync(Guid id);
}