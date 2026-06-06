using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByUniqueNameAndPasswordCommand : IRequest<IResult<AuthenticatedUserResponse>> {
    public required string UniqueName { get; init; }
    public required string Password { get; init; }

    [SetsRequiredMembers]
    public LoginOneUserByUniqueNameAndPasswordCommand(LoginOneUserByUniqueNameAndPasswordCommandArgs args) {
        this.UniqueName = args.UniqueName;
        this.Password = args.Password;
    }
}