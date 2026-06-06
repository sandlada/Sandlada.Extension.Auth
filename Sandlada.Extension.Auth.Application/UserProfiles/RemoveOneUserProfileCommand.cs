using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record RemoveOneUserProfileCommand : IRequest<IResult<RemoveOneUserProfileCommandResponse>> {
    public required Guid UserId { get; init; }

    [SetsRequiredMembers]
    public RemoveOneUserProfileCommand(RemoveOneUserProfileCommandArgs args) {
        this.UserId = args.UserId;
    }
}
