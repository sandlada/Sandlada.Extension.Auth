using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed class FindManyOAuthClientQueryHandler(
    IOAuthClientRepository oauthClientRepository
) : IRequestHandler<FindManyOAuthClientQuery, IResult<List<FindOneOAuthClientByClientIdQueryResponse>>> {

    public async Task<IResult<List<FindOneOAuthClientByClientIdQueryResponse>>> Handle(FindManyOAuthClientQuery request, CancellationToken cancellationToken) {
        var result = await oauthClientRepository.FindManyAsync();
        if (result.IsFailure) {
            return Result.Failure<List<FindOneOAuthClientByClientIdQueryResponse>>(result.Error);
        }

        var responses = result.Value.Select(FindOneOAuthClientByClientIdQueryResponse.From).ToList();
        return Result.Success(responses);
    }
}