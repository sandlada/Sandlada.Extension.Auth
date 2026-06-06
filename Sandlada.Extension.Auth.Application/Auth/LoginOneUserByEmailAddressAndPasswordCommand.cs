using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByEmailAddressAndPasswordCommand : IRequest<IResult<AuthenticatedUserResponse>> {
    public required string EmailAddress { get; init; }
    public required string Password { get; init; }

    [SetsRequiredMembers]
    public LoginOneUserByEmailAddressAndPasswordCommand(LoginOneUserByEmailAddressAndPasswordCommandArgs args) {
        this.EmailAddress = args.EmailAddress;
        this.Password = args.Password;
    }
}