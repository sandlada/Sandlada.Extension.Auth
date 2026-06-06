using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByUniqueNameCommand : IRequest<IResult<AuthenticatedUserResponse>> {
    public required string UniqueName { get; init; }
    public required string Password { get; init; }

    [SetsRequiredMembers]
    public LoginOneUserByUniqueNameCommand(LoginOneUserByUniqueNameCommandArgs args) {
        this.UniqueName = args.UniqueName;
        this.Password = args.Password;
    }
}
