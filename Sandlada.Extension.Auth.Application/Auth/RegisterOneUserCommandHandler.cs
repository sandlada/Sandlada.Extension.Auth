using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Application.UserProfiles;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class RegisterOneUserCommandHandler(
    IUserRepository userRepository,
    IRegistrationVerificationRepository registrationVerificationRepository,
    ISecretHashService secretHashService,
    FirstLoginUserProfileInitializer firstLoginUserProfileInitializer,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RegisterOneUserCommand, IResult<AuthenticatedUserResponse>> {

    public async Task<IResult<AuthenticatedUserResponse>> Handle(RegisterOneUserCommand request, CancellationToken cancellationToken) {
        var emailAddressResult = EmailAddress.From(request.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(emailAddressResult.Error);
        if (string.IsNullOrWhiteSpace(request.VerificationCode)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidVerificationCode);
        if (string.IsNullOrWhiteSpace(request.Password)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.PasswordCannotBeEmpty);

        var emailAddress = emailAddressResult.Value;
        var utcNow = DateTime.UtcNow;

        var userResult = await userRepository.FindOneByEmailAddressAsync(emailAddress);
        if (userResult.IsFailure) {
            if (userResult.Error == DomainError.User.NotFound) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeNotFound);
            return Result.Failure<AuthenticatedUserResponse>(userResult.Error);
        }

        var user = userResult.Value;
        if (user.IsEmailVerified && !string.IsNullOrWhiteSpace(user.UniqueName)) {
            return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.RegistrationProfileAlreadyCompleted);
        }

        var verificationResult = await registrationVerificationRepository.FindOneByEmailAddressAsync(emailAddress);
        if (verificationResult.IsFailure) {
            if (verificationResult.Error == DomainError.Auth.VerificationCodeNotFound) return Result.Failure<AuthenticatedUserResponse>(verificationResult.Error);
            return Result.Failure<AuthenticatedUserResponse>(verificationResult.Error);
        }

        var verification = verificationResult.Value;
        if (verification.IsConsumed) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeAlreadyUsed);
        if (verification.IsExpired(utcNow)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeExpired);
        if (verification.IsFailedAttemptLimitExceeded) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeAttemptLimitExceeded);
        if (!secretHashService.Verify(request.VerificationCode, verification.VerificationCodeHash)) {
            var registerFailedAttemptResult = verification.RegisterFailedAttempt(utcNow);
            if (registerFailedAttemptResult.IsSuccess || registerFailedAttemptResult.Error == DomainError.Auth.VerificationCodeAttemptLimitExceeded) {
                var updateFailedAttemptResult = await registrationVerificationRepository.UpdateOneAsync(verification);
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

        var normalizedUniqueName = request.UniqueName.Trim();
        var existingUniqueNameResult = await userRepository.FindOneByUniqueNameAsync(normalizedUniqueName);
        if (existingUniqueNameResult.IsSuccess && existingUniqueNameResult.Value.Id != user.Id) {
            return Result.Failure<AuthenticatedUserResponse>(DomainError.User.UniqueNameAlreadyExists);
        }

        if (existingUniqueNameResult.IsFailure && existingUniqueNameResult.Error != DomainError.User.NotFound) {
            return Result.Failure<AuthenticatedUserResponse>(existingUniqueNameResult.Error);
        }

        var updateUniqueNameResult = user.UpdateUniqueName(normalizedUniqueName);
        if (updateUniqueNameResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateUniqueNameResult.Error);

        if (!secretHashService.Verify(request.Password, user.PasswordHash)) {
            var passwordHash = secretHashService.Hash(request.Password);
            var updatePasswordResult = user.UpdatePasswordHash(passwordHash);
            if (updatePasswordResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updatePasswordResult.Error);
        }

        var updateEmailVerifiedResult = user.UpdateIsEmailVerified(true);
        if (updateEmailVerifiedResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateEmailVerifiedResult.Error);

        var initializeResult = await firstLoginUserProfileInitializer.InitializeAsync(user, cancellationToken);
        if (initializeResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(initializeResult.Error);

        var repositoryUpdateUserResult = await userRepository.UpdateOneAsync(user);
        if (repositoryUpdateUserResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(repositoryUpdateUserResult.Error);

        var updateVerificationResult = await registrationVerificationRepository.UpdateOneAsync(verification);
        if (updateVerificationResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateVerificationResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(AuthenticatedUserResponse.From(user));
    }
}