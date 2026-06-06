using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed record FindOneCurrentUserUserStatusQuery : IRequest<IResult<UserStatusResponse>> {
    public required Guid UserId { get; init; }

    [SetsRequiredMembers]
    public FindOneCurrentUserUserStatusQuery(Guid userId) {
        this.UserId = userId;
    }
}
