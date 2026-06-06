using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record UpdateOneUserProfileCommand : IRequest<IResult<UserProfileResponse>> {
    public required Guid UserId { get; init; }
    public required UpdateOneUserProfileCommandArgs Args { get; init; }

    [SetsRequiredMembers]
    public UpdateOneUserProfileCommand(Guid userId, UpdateOneUserProfileCommandArgs args) {
        this.UserId = userId;
        this.Args = args;
    }
}
