using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed class UpdateOneUserUserStatusCommandHandler(
    IUserRepository userRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<UpdateOneUserUserStatusCommand, IResult<UserResponse>> {

    public async Task<IResult<UserResponse>> Handle(UpdateOneUserUserStatusCommand request, CancellationToken cancellationToken) {
        var userResult = await userRepository.FindOneByIdAsync(request.UserId);
        if (userResult.IsFailure) return Result.Failure<UserResponse>(userResult.Error);

        var user = userResult.Value;

        var statusResult = UserStatus.From(new UserStatusConstructorArgs { Code = request.Status });
        if (statusResult.IsFailure) return Result.Failure<UserResponse>(statusResult.Error);

        var updateStatusResult = user.UpdateStatus(statusResult.Value);
        if (updateStatusResult.IsFailure) return Result.Failure<UserResponse>(updateStatusResult.Error);

        var updateUserResult = await userRepository.UpdateOneAsync(user);
        if (updateUserResult.IsFailure) return Result.Failure<UserResponse>(updateUserResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(UserResponse.From(user));
    }
}
