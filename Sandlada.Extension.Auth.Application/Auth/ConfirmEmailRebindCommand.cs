using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record ConfirmEmailRebindCommand : IRequest<IResult<AuthenticatedUserResponse>> {
    public required Guid UserId { get; init; }
    public required string EmailAddress { get; init; }
    public required string VerificationCode { get; init; }

    [SetsRequiredMembers]
    public ConfirmEmailRebindCommand(Guid userId, ConfirmEmailRebindCommandArgs args) {
        this.UserId = userId;
        this.EmailAddress = args.EmailAddress;
        this.VerificationCode = args.VerificationCode;
    }
}
