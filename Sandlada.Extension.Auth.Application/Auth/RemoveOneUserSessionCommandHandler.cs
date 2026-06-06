using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class RemoveOneUserSessionCommandHandler(
    IAuthSessionRepository authSessionRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RemoveOneUserSessionCommand, IResult<RemoveOneUserSessionCommandResponse>> {

    public async Task<IResult<RemoveOneUserSessionCommandResponse>> Handle(RemoveOneUserSessionCommand request, CancellationToken cancellationToken) {
        var removeResult = await authSessionRepository.RemoveOneBySessionIdAsync(request.SessionId);
        if (removeResult.IsFailure) return Result.Failure<RemoveOneUserSessionCommandResponse>(removeResult.Error);
        if (removeResult.Value <= 0) return Result.Failure<RemoveOneUserSessionCommandResponse>(DomainError.Auth.SessionNotFound);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new RemoveOneUserSessionCommandResponse { Removed = true });
    }
}
