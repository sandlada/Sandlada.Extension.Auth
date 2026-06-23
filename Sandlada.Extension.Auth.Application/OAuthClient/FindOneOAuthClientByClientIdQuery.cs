using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record FindOneOAuthClientByClientIdQuery : IRequest<IResult<FindOneOAuthClientByClientIdQueryResponse>> {
    public required string ClientId { get; init; }

    [SetsRequiredMembers]
    public FindOneOAuthClientByClientIdQuery(FindOneOAuthClientByClientIdQueryArgs args) {
        this.ClientId = args.ClientId;
    }
}