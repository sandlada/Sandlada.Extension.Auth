using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RemoveOneUserSessionCommand : IRequest<IResult<RemoveOneUserSessionCommandResponse>> {
    public required string SessionId { get; init; }

    [SetsRequiredMembers]
    public RemoveOneUserSessionCommand(RemoveOneUserSessionCommandArgs args) {
        this.SessionId = args.SessionId;
    }
}
