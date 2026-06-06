using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed class FirstLoginUserProfileInitializer(
    IUserProfileRepository UserProfileRepository
) {
    public async Task<IResult<bool>> InitializeAsync(User user, CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        if (user.FirstLoginAt is not null) {
            return Result.Success(false);
        }

        var existingUserProfileResult = await UserProfileRepository.FindOneByUserIdAsync(user.Id);
        if (existingUserProfileResult.IsFailure) {
            if (existingUserProfileResult.Error != DomainError.UserProfile.NotFound) {
                return Result.Failure<bool>(existingUserProfileResult.Error);
            }

            var createResult = UserProfileMutationCommandHelper.CreateOne(user.Id, new InsertOrUpdateOneUserProfileCommandArgs());
            if (createResult.IsFailure) return Result.Failure<bool>(createResult.Error);

            var insertResult = await UserProfileRepository.InsertOneAsync(createResult.Value);
            if (insertResult.IsFailure) return Result.Failure<bool>(insertResult.Error);
        }

        var markFirstLoginResult = user.MarkFirstLogin(DateTime.UtcNow);
        if (markFirstLoginResult.IsFailure) return Result.Failure<bool>(markFirstLoginResult.Error);

        return Result.Success(true);
    }
}
