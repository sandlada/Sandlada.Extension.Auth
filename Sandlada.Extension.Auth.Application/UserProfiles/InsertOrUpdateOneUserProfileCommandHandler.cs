using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed class InsertOrUpdateOneUserProfileCommandHandler(
    IUserProfileRepository UserProfileRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<InsertOrUpdateOneUserProfileCommand, IResult<UserProfileResponse>> {

    public async Task<IResult<UserProfileResponse>> Handle(InsertOrUpdateOneUserProfileCommand request, CancellationToken cancellationToken) {
        var existingUserProfileResult = await UserProfileRepository.FindOneByUserIdAsync(request.UserId);

        if (existingUserProfileResult.IsSuccess) {
            var UserProfile = existingUserProfileResult.Value;
            var applyUpdateResult = UserProfileMutationCommandHelper.ApplyUpdates(UserProfile, request.Args);
            if (applyUpdateResult.IsFailure) return Result.Failure<UserProfileResponse>(applyUpdateResult.Error);

            var updateResult = await UserProfileRepository.UpdateOneAsync(UserProfile);
            if (updateResult.IsFailure) return Result.Failure<UserProfileResponse>(updateResult.Error);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success(UserProfileResponse.From(UserProfile));
        }

        if (existingUserProfileResult.Error != DomainError.UserProfile.NotFound) {
            return Result.Failure<UserProfileResponse>(existingUserProfileResult.Error);
        }

        var createResult = UserProfileMutationCommandHelper.CreateOne(request.UserId, request.Args);
        if (createResult.IsFailure) return Result.Failure<UserProfileResponse>(createResult.Error);

        var insertResult = await UserProfileRepository.InsertOneAsync(createResult.Value);
        if (insertResult.IsFailure) return Result.Failure<UserProfileResponse>(insertResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(UserProfileResponse.From(createResult.Value));
    }
}
