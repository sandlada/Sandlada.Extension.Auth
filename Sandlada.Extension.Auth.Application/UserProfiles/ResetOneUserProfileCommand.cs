using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed record ResetOneUserProfileCommand : IRequest<IResult<UserProfileResponse>> {
    public required Guid UserId { get; init; }

    [SetsRequiredMembers]
    public ResetOneUserProfileCommand(ResetOneUserProfileCommandArgs args) {
        this.UserId = args.UserId;
    }
}
