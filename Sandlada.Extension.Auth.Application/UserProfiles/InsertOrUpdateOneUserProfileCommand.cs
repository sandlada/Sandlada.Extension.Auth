using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record InsertOrUpdateOneUserProfileCommand : IRequest<IResult<UserProfileResponse>> {
    public required Guid UserId { get; init; }
    public required InsertOrUpdateOneUserProfileCommandArgs Args { get; init; }

    [SetsRequiredMembers]
    public InsertOrUpdateOneUserProfileCommand(Guid userId, InsertOrUpdateOneUserProfileCommandArgs args) {
        this.UserId = userId;
        this.Args = args;
    }
}
