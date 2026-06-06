using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.UserProfiles;

public sealed class FindOneUserProfileByUserIdQueryHandler(
    IUserProfileRepository UserProfileRepository
) : IRequestHandler<FindOneUserProfileByUserIdQuery, IResult<UserProfileResponse>> {

    public async Task<IResult<UserProfileResponse>> Handle(FindOneUserProfileByUserIdQuery request, CancellationToken cancellationToken) {
        var UserProfileResult = await UserProfileRepository.FindOneByUserIdAsync(request.UserId);
        if (UserProfileResult.IsFailure) return Result.Failure<UserProfileResponse>(UserProfileResult.Error);

        return Result.Success(UserProfileResponse.From(UserProfileResult.Value));
    }
}
