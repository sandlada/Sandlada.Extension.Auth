using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed record UpdateOneUserUserStatusCommand : IRequest<IResult<UserResponse>> {
    public required Guid UserId { get; init; }
    public required string Status { get; init; }

    [SetsRequiredMembers]
    public UpdateOneUserUserStatusCommand(Guid userId, string status) {
        this.UserId = userId;
        this.Status = status;
    }
}
