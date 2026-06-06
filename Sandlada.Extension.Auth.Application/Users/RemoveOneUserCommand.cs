using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed record RemoveOneUserCommand : IRequest<IResult<RemoveOneUserCommandResponse>> {
    public required Guid UserId { get; init; }

    [SetsRequiredMembers]
    public RemoveOneUserCommand(RemoveOneUserCommandArgs args) {
        this.UserId = args.UserId;
    }
}
