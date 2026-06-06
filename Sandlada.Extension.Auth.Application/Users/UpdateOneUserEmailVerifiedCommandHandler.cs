using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed class UpdateOneUserEmailVerifiedCommandHandler(
    IUserRepository userRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<UpdateOneUserEmailVerifiedCommand, IResult<UserResponse>> {

    public async Task<IResult<UserResponse>> Handle(UpdateOneUserEmailVerifiedCommand request, CancellationToken cancellationToken) {
        var userResult = await userRepository.FindOneByIdAsync(request.UserId);
        if (userResult.IsFailure) return Result.Failure<UserResponse>(userResult.Error);

        var user = userResult.Value;
        var updateResult = user.UpdateIsEmailVerified(request.IsEmailVerified);
        if (updateResult.IsFailure) return Result.Failure<UserResponse>(updateResult.Error);

        var repositoryUpdateResult = await userRepository.UpdateOneAsync(user);
        if (repositoryUpdateResult.IsFailure) return Result.Failure<UserResponse>(repositoryUpdateResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(UserResponse.From(user));
    }
}
