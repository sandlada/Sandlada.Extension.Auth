using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RequestEmailRebindVerificationCodeCommand : IRequest<IResult<RequestEmailRebindVerificationCodeCommandResponse>> {
    public required Guid UserId { get; init; }
    public required string EmailAddress { get; init; }

    [SetsRequiredMembers]
    public RequestEmailRebindVerificationCodeCommand(Guid userId, RequestEmailRebindVerificationCodeCommandArgs args) {
        this.UserId = userId;
        this.EmailAddress = args.EmailAddress;
    }
}
