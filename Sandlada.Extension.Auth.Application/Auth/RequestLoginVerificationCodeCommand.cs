using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RequestLoginVerificationCodeCommand : IRequest<IResult<RequestLoginVerificationCodeCommandResponse>> {
    public string? EmailAddress { get; init; }
    public string? UniqueName { get; init; }

    [SetsRequiredMembers]
    public RequestLoginVerificationCodeCommand(RequestLoginVerificationCodeCommandArgs args) {
        this.EmailAddress = args.EmailAddress;
        this.UniqueName = args.UniqueName;
    }
}