using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed class RemoveOneUserProfileCommandHandler(
    IUserProfileRepository UserProfileRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RemoveOneUserProfileCommand, IResult<RemoveOneUserProfileCommandResponse>> {

    public async Task<IResult<RemoveOneUserProfileCommandResponse>> Handle(RemoveOneUserProfileCommand request, CancellationToken cancellationToken) {
        var removeResult = await UserProfileRepository.RemoveOneByUserIdAsync(request.UserId);
        if (removeResult.IsFailure) return Result.Failure<RemoveOneUserProfileCommandResponse>(removeResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new RemoveOneUserProfileCommandResponse {
            Removed = true,
        });
    }
}
