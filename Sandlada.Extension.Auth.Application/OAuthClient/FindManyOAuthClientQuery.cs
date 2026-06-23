using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;

namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record FindManyOAuthClientQuery : IRequest<IResult<List<FindOneOAuthClientByClientIdQueryResponse>>> {
}