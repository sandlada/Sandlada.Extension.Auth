using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed class FindOneCurrentUserUserStatusQueryHandler(
    IUserRepository userRepository
) : IRequestHandler<FindOneCurrentUserUserStatusQuery, IResult<UserStatusResponse>> {

    public async Task<IResult<UserStatusResponse>> Handle(FindOneCurrentUserUserStatusQuery request, CancellationToken cancellationToken) {
        var userResult = await userRepository.FindOneByIdAsync(request.UserId);
        if (userResult.IsFailure) return Result.Failure<UserStatusResponse>(userResult.Error);

        return Result.Success(new UserStatusResponse {
            Status = userResult.Value.Status.Code,
        });
    }
}
