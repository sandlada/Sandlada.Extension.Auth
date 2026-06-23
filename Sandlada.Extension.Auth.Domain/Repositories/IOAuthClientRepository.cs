using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.Repositories;

public interface IOAuthClientRepository {
    Task<IResult> InsertOneAsync(OAuthClient client);
    Task<IResult<OAuthClient>> FindOneByIdAsync(Guid id);
    Task<IResult<OAuthClient>> FindOneByClientIdAsync(string clientId);
    Task<IResult<List<OAuthClient>>> FindManyAsync();
    Task<IResult> UpdateOneAsync(OAuthClient client);
    Task<IResult> RemoveOneAsync(Guid id);
}