using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.Repositories;

public interface IUserProfileRepository {
    Task<IResult> InsertOneAsync(UserProfile userProfile);

    Task<IResult<UserProfile>> FindOneByUserIdAsync(Guid userId);

    Task<IResult> UpdateOneAsync(UserProfile userProfile);

    Task<IResult> RemoveOneByUserIdAsync(Guid userId);
}
