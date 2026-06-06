using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class RemoveManyUserSessionsByUserIdCommandHandler(
    IAuthSessionRepository authSessionRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RemoveManyUserSessionsByUserIdCommand, IResult<RemoveManyUserSessionsByUserIdCommandResponse>> {

    public async Task<IResult<RemoveManyUserSessionsByUserIdCommandResponse>> Handle(RemoveManyUserSessionsByUserIdCommand request, CancellationToken cancellationToken) {
        var removeResult = await authSessionRepository.RemoveManyByUserIdAsync(request.UserId);
        if (removeResult.IsFailure) return Result.Failure<RemoveManyUserSessionsByUserIdCommandResponse>(removeResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new RemoveManyUserSessionsByUserIdCommandResponse { RemovedCount = removeResult.Value });
    }
}
