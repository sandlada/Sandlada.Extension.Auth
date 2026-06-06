using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RegisterOneUserCommand : IRequest<IResult<AuthenticatedUserResponse>> {
    public required string EmailAddress { get; init; }
    public required string VerificationCode { get; init; }
    public required string UniqueName { get; init; }
    public required string Password { get; init; }

    [SetsRequiredMembers]
    public RegisterOneUserCommand(RegisterOneUserCommandArgs args) {
        this.EmailAddress = args.EmailAddress;
        this.VerificationCode = args.VerificationCode;
        this.UniqueName = args.UniqueName;
        this.Password = args.Password;
    }
}
