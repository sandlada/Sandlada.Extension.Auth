using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record InsertOneUserProfileCommand : IRequest<IResult<UserProfileResponse>> {
    public required Guid UserId { get; init; }
    public required InsertOneUserProfileCommandArgs Args { get; init; }

    [SetsRequiredMembers]
    public InsertOneUserProfileCommand(Guid userId, InsertOneUserProfileCommandArgs args) {
        this.UserId = userId;
        this.Args = args;
    }
}
