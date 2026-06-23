using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed class FindOneOAuthClientByClientIdQueryHandler(
    IOAuthClientRepository oauthClientRepository
) : IRequestHandler<FindOneOAuthClientByClientIdQuery, IResult<FindOneOAuthClientByClientIdQueryResponse>> {

    public async Task<IResult<FindOneOAuthClientByClientIdQueryResponse>> Handle(FindOneOAuthClientByClientIdQuery request, CancellationToken cancellationToken) {
        var result = await oauthClientRepository.FindOneByClientIdAsync(request.ClientId);
        if (result.IsFailure) {
            return Result.Failure<FindOneOAuthClientByClientIdQueryResponse>(result.Error);
        }

        return Result.Success(FindOneOAuthClientByClientIdQueryResponse.From(result.Value));
    }
}