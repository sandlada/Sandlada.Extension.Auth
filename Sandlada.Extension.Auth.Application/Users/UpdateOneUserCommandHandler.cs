using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed class UpdateOneUserCommandHandler(
    IUserRepository userRepository,
    ISecretHashService secretHashService,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<UpdateOneUserCommand, IResult<UserResponse>> {

    public async Task<IResult<UserResponse>> Handle(UpdateOneUserCommand request, CancellationToken cancellationToken) {
        var userResult = await userRepository.FindOneByIdAsync(request.UserId);
        if (userResult.IsFailure) return Result.Failure<UserResponse>(userResult.Error);

        var user = userResult.Value;
        UserStatus? status = null;
        if (request.Status is not null) {
            var statusResult = UserStatus.From(new UserStatusConstructorArgs { Code = request.Status });
            if (statusResult.IsFailure) return Result.Failure<UserResponse>(statusResult.Error);
            status = statusResult.Value;
        }

        var applyUpdatesResult = await UserMutationCommandHelper.ApplyUpdatesAsync(
            userRepository,
            secretHashService,
            user,
            request.EmailAddress,
            request.UniqueName,
            request.Role,
            request.Password,
            request.IsEmailVerified,
            status
        );
        if (applyUpdatesResult.IsFailure) return Result.Failure<UserResponse>(applyUpdatesResult.Error);

        var updateUserResult = await userRepository.UpdateOneAsync(user);
        if (updateUserResult.IsFailure) return Result.Failure<UserResponse>(updateUserResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(UserResponse.From(user));
    }
}
