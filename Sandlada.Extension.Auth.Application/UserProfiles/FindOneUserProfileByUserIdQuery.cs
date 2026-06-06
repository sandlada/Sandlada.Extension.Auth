using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record FindOneUserProfileByUserIdQuery : IRequest<IResult<UserProfileResponse>> {
    public required Guid UserId { get; init; }

    [SetsRequiredMembers]
    public FindOneUserProfileByUserIdQuery(FindOneUserProfileByUserIdQueryArgs args) {
        this.UserId = args.UserId;
    }
}
