using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed class ResetOneUserProfileCommandHandler(
    IUserProfileRepository UserProfileRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<ResetOneUserProfileCommand, IResult<UserProfileResponse>> {

    public async Task<IResult<UserProfileResponse>> Handle(ResetOneUserProfileCommand request, CancellationToken cancellationToken) {
        var UserProfileResult = await UserProfileRepository.FindOneByUserIdAsync(request.UserId);
        if (UserProfileResult.IsFailure) return Result.Failure<UserProfileResponse>(UserProfileResult.Error);

        var UserProfile = UserProfileResult.Value;
        var resetResult = UserProfile.ResetToDefaults();
        if (resetResult.IsFailure) return Result.Failure<UserProfileResponse>(resetResult.Error);

        var updateResult = await UserProfileRepository.UpdateOneAsync(UserProfile);
        if (updateResult.IsFailure) return Result.Failure<UserProfileResponse>(updateResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(UserProfileResponse.From(UserProfile));
    }
}
