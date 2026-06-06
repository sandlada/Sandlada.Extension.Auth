using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RemoveManyUserSessionsByUserIdCommand : IRequest<IResult<RemoveManyUserSessionsByUserIdCommandResponse>> {
    public required Guid UserId { get; init; }

    [SetsRequiredMembers]
    public RemoveManyUserSessionsByUserIdCommand(RemoveManyUserSessionsByUserIdCommandArgs args) {
        this.UserId = args.UserId;
    }
}
