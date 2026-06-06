using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Repositories;

public interface IUserRepository {
    Task<IResult> InsertOneAsync(User user);

    Task<IResult<User>> FindOneByIdAsync(Guid id);
    Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress);
    Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName);

    Task<IResult> UpdateOneAsync(User user);

    Task<IResult> RemoveOneAsync(Guid id);
}
