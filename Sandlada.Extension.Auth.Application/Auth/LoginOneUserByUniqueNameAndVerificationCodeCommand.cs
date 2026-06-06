using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record LoginOneUserByUniqueNameAndVerificationCodeCommand : IRequest<IResult<AuthenticatedUserResponse>> {
    public required string UniqueName { get; init; }
    public required string VerificationCode { get; init; }

    [SetsRequiredMembers]
    public LoginOneUserByUniqueNameAndVerificationCodeCommand(LoginOneUserByUniqueNameAndVerificationCodeCommandArgs args) {
        this.UniqueName = args.UniqueName;
        this.VerificationCode = args.VerificationCode;
    }
}