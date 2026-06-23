using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record RemoveOneOAuthClientCommand : IRequest<IResult> {
    public required Guid Id { get; init; }

    [SetsRequiredMembers]
    public RemoveOneOAuthClientCommand(RemoveOneOAuthClientCommandArgs args) {
        this.Id = args.Id;
    }
}