using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Application.UserProfiles;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class LoginOneUserByEmailAddressAndVerificationCodeCommandHandler(
    IUserRepository userRepository,
    ILoginVerificationRepository loginVerificationRepository,
    ISecretHashService secretHashService,
    FirstLoginUserProfileInitializer firstLoginUserProfileInitializer,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<LoginOneUserByEmailAddressAndVerificationCodeCommand, IResult<AuthenticatedUserResponse>> {

    public async Task<IResult<AuthenticatedUserResponse>> Handle(LoginOneUserByEmailAddressAndVerificationCodeCommand request, CancellationToken cancellationToken) {
        var emailAddressResult = EmailAddress.From(request.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);
        if (string.IsNullOrWhiteSpace(request.VerificationCode)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidVerificationCode);

        var emailAddress = emailAddressResult.Value;
        var utcNow = DateTime.UtcNow;

        var userResult = await userRepository.FindOneByEmailAddressAsync(emailAddress);
        if (userResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);

        var user = userResult.Value;
        if (!user.IsEmailVerified) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.EmailAddressNotVerified);

        var verificationResult = await loginVerificationRepository.FindOneByEmailAddressAsync(emailAddress);
        if (verificationResult.IsFailure) {
            if (verificationResult.Error == DomainError.Auth.VerificationCodeNotFound) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidVerificationCode);
            return Result.Failure<AuthenticatedUserResponse>(verificationResult.Error);
        }

        var verification = verificationResult.Value;
        if (verification.IsConsumed) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeAlreadyUsed);
        if (verification.IsExpired(utcNow)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeExpired);
        if (verification.IsFailedAttemptLimitExceeded) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeAttemptLimitExceeded);

        if (!secretHashService.Verify(request.VerificationCode, verification.VerificationCodeHash)) {
            var registerFailedAttemptResult = verification.RegisterFailedAttempt(utcNow);
            if (registerFailedAttemptResult.IsSuccess || registerFailedAttemptResult.Error == DomainError.Auth.VerificationCodeAttemptLimitExceeded) {
                var updateFailedAttemptResult = await loginVerificationRepository.UpdateOneAsync(verification);
                if (updateFailedAttemptResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateFailedAttemptResult.Error);

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            if (registerFailedAttemptResult.IsFailure && registerFailedAttemptResult.Error != DomainError.Auth.VerificationCodeAttemptLimitExceeded) {
                return Result.Failure<AuthenticatedUserResponse>(registerFailedAttemptResult.Error);
            }

            if (verification.IsFailedAttemptLimitExceeded) {
                return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeAttemptLimitExceeded);
            }

            return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidVerificationCode);
        }

        var consumeResult = verification.Consume(utcNow);
        if (consumeResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(consumeResult.Error);

        var initializeFirstLoginResult = await firstLoginUserProfileInitializer.InitializeAsync(user, cancellationToken);
        if (initializeFirstLoginResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(initializeFirstLoginResult.Error);

        if (initializeFirstLoginResult.Value) {
            var updateUserResult = await userRepository.UpdateOneAsync(user);
            if (updateUserResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateUserResult.Error);
        }

        var updateVerificationResult = await loginVerificationRepository.UpdateOneAsync(verification);
        if (updateVerificationResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateVerificationResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(AuthenticatedUserResponse.From(user));
    }
}