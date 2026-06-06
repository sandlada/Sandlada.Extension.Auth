using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed class FindOneUserByIdQueryHandler(IUserRepository userRepository) : IRequestHandler<FindOneUserByIdQuery, IResult<UserResponse>> {

    public async Task<IResult<UserResponse>> Handle(FindOneUserByIdQuery request, CancellationToken cancellationToken) {
        var userResult = await userRepository.FindOneByIdAsync(request.UserId);
        if (userResult.IsFailure) return Result.Failure<UserResponse>(userResult.Error);

        return Result.Success(UserResponse.From(userResult.Value));
    }
}
