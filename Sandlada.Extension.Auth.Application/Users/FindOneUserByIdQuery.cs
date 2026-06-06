using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed record FindOneUserByIdQuery : IRequest<IResult<UserResponse>> {
    public required Guid UserId { get; init; }

    [SetsRequiredMembers]
    public FindOneUserByIdQuery(FindOneUserByIdQueryArgs args) {
        this.UserId = args.UserId;
    }
}
