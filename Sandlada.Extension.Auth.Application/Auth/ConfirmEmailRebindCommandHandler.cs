using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class ConfirmEmailRebindCommandHandler(
    IUserRepository userRepository,
    IEmailRebindVerificationRepository emailRebindVerificationRepository,
    ISecretHashService secretHashService,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<ConfirmEmailRebindCommand, IResult<AuthenticatedUserResponse>> {

    public async Task<IResult<AuthenticatedUserResponse>> Handle(ConfirmEmailRebindCommand request, CancellationToken cancellationToken) {
        var targetEmailAddressResult = EmailAddress.From(request.EmailAddress);
        if (targetEmailAddressResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(targetEmailAddressResult.Error);
        if (string.IsNullOrWhiteSpace(request.VerificationCode)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidVerificationCode);

        var userResult = await userRepository.FindOneByIdAsync(request.UserId);
        if (userResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(userResult.Error);

        var user = userResult.Value;
        if (!user.IsEmailVerified) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.EmailAddressNotVerified);

        var targetEmailAddress = targetEmailAddressResult.Value;
        var verificationResult = await emailRebindVerificationRepository.FindOneByUserIdAsync(user.Id);
        if (verificationResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(verificationResult.Error);

        var verification = verificationResult.Value;
        if (!verification.TargetEmailAddress.Equals(targetEmailAddress)) {
            return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidVerificationCode);
        }

        var utcNow = DateTime.UtcNow;
        if (verification.IsConsumed) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeAlreadyUsed);
        if (verification.IsExpired(utcNow)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeExpired);
        if (verification.IsFailedAttemptLimitExceeded) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.VerificationCodeAttemptLimitExceeded);
        if (!secretHashService.Verify(request.VerificationCode, verification.VerificationCodeHash)) {
            var registerFailedAttemptResult = verification.RegisterFailedAttempt(utcNow);
            if (registerFailedAttemptResult.IsSuccess || registerFailedAttemptResult.Error == DomainError.Auth.VerificationCodeAttemptLimitExceeded) {
                var updateFailedAttemptResult = await emailRebindVerificationRepository.UpdateOneAsync(verification);
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

        var existingEmailUserResult = await userRepository.FindOneByEmailAddressAsync(targetEmailAddress);
        if (existingEmailUserResult.IsSuccess && existingEmailUserResult.Value.Id != user.Id) {
            return Result.Failure<AuthenticatedUserResponse>(DomainError.User.EmailAddressAlreadyExists);
        }

        if (existingEmailUserResult.IsFailure && existingEmailUserResult.Error != DomainError.User.NotFound) {
            return Result.Failure<AuthenticatedUserResponse>(existingEmailUserResult.Error);
        }

        var consumeResult = verification.Consume(utcNow);
        if (consumeResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(consumeResult.Error);

        var updateEmailAddressResult = user.UpdateEmailAddress(targetEmailAddress);
        if (updateEmailAddressResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateEmailAddressResult.Error);

        var markVerifiedResult = user.UpdateIsEmailVerified(true);
        if (markVerifiedResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(markVerifiedResult.Error);

        var updateUserResult = await userRepository.UpdateOneAsync(user);
        if (updateUserResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateUserResult.Error);

        var updateVerificationResult = await emailRebindVerificationRepository.UpdateOneAsync(verification);
        if (updateVerificationResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateVerificationResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(AuthenticatedUserResponse.From(user));
    }
}
