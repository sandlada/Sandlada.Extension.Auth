using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed class UpdateOneUserProfileCommandHandler(
    IUserProfileRepository UserProfileRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<UpdateOneUserProfileCommand, IResult<UserProfileResponse>> {

    public async Task<IResult<UserProfileResponse>> Handle(UpdateOneUserProfileCommand request, CancellationToken cancellationToken) {
        var UserProfileResult = await UserProfileRepository.FindOneByUserIdAsync(request.UserId);
        if (UserProfileResult.IsFailure) return Result.Failure<UserProfileResponse>(UserProfileResult.Error);

        var UserProfile = UserProfileResult.Value;
        var updateResult = UserProfileMutationCommandHelper.ApplyUpdates(UserProfile, request.Args);
        if (updateResult.IsFailure) return Result.Failure<UserProfileResponse>(updateResult.Error);

        var repositoryUpdateResult = await UserProfileRepository.UpdateOneAsync(UserProfile);
        if (repositoryUpdateResult.IsFailure) return Result.Failure<UserProfileResponse>(repositoryUpdateResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(UserProfileResponse.From(UserProfile));
    }
}
