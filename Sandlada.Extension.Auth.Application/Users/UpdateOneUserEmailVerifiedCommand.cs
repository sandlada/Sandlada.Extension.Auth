using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed record UpdateOneUserEmailVerifiedCommand : IRequest<IResult<UserResponse>> {
    public required Guid UserId { get; init; }
    public required bool IsEmailVerified { get; init; }

    [SetsRequiredMembers]
    public UpdateOneUserEmailVerifiedCommand(UpdateOneUserEmailVerifiedCommandArgs args) {
        this.UserId = args.UserId;
        this.IsEmailVerified = args.IsEmailVerified;
    }
}
