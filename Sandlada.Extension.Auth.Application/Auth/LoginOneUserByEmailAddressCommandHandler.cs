using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Application.UserProfiles;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class LoginOneUserByEmailAddressCommandHandler(
    IUserRepository userRepository,
    ISecretHashService secretHashService,
    FirstLoginUserProfileInitializer firstLoginUserProfileInitializer,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<LoginOneUserByEmailAddressCommand, IResult<AuthenticatedUserResponse>> {

    public async Task<IResult<AuthenticatedUserResponse>> Handle(LoginOneUserByEmailAddressCommand request, CancellationToken cancellationToken) {
        var emailAddressResult = EmailAddress.From(request.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);
        if (string.IsNullOrWhiteSpace(request.Password)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);

        var userResult = await userRepository.FindOneByEmailAddressAsync(emailAddressResult.Value);
        if (userResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);

        var user = userResult.Value;
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
