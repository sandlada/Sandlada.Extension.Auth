using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByEmailAddressAndVerificationCodeCommand : IRequest<IResult<AuthenticatedUserResponse>> {
    public required string EmailAddress { get; init; }
    public required string VerificationCode { get; init; }

    [SetsRequiredMembers]
    public LoginOneUserByEmailAddressAndVerificationCodeCommand(LoginOneUserByEmailAddressAndVerificationCodeCommandArgs args) {
        this.EmailAddress = args.EmailAddress;
        this.VerificationCode = args.VerificationCode;
    }
}