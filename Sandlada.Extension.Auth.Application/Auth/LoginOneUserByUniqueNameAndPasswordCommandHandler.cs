using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Application.UserProfiles;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class LoginOneUserByUniqueNameAndPasswordCommandHandler(
    IUserRepository userRepository,
    ISecretHashService secretHashService,
    FirstLoginUserProfileInitializer firstLoginUserProfileInitializer,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<LoginOneUserByUniqueNameAndPasswordCommand, IResult<AuthenticatedUserResponse>> {

    public async Task<IResult<AuthenticatedUserResponse>> Handle(LoginOneUserByUniqueNameAndPasswordCommand request, CancellationToken cancellationToken) {
        if (string.IsNullOrWhiteSpace(request.UniqueName) || string.IsNullOrWhiteSpace(request.Password)) {
            return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);
        }

        var userResult = await userRepository.FindOneByUniqueNameAsync(request.UniqueName);
        if (userResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);

        var user = userResult.Value;
        if (string.IsNullOrWhiteSpace(user.UniqueName)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);
        if (!user.IsEmailVerified) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.EmailAddressNotVerified);
        if (!secretHashService.Verify(request.Password, user.PasswordHash)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);

        var initializeFirstLoginResult = await firstLoginUserProfileInitializer.InitializeAsync(user, cancellationToken);
        if (initializeFirstLoginResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(initializeFirstLoginResult.Error);

        if (initializeFirstLoginResult.Value) {
            var updateUserResult = await userRepository.UpdateOneAsync(user);
            if (updateUserResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateUserResult.Error);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Result.Success(AuthenticatedUserResponse.From(user));
    }
}