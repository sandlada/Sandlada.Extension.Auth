using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Repositories;

public interface IPasswordLoginAttemptRepository {
    Task<IResult> InsertOneAsync(PasswordLoginAttempt attempt);

    Task<IResult<PasswordLoginAttempt>> FindOneByEmailAddressAsync(EmailAddress emailAddress);

    Task<IResult> UpdateOneAsync(PasswordLoginAttempt attempt);

    Task<IResult> RemoveOneAsync(Guid id);
}
