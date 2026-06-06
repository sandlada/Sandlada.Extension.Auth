using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class RequestEmailRebindVerificationCodeCommandHandler(
    IUserRepository userRepository,
    IEmailRebindVerificationRepository emailRebindVerificationRepository,
    IRegistrationVerificationCodeGenerator verificationCodeGenerator,
    IRegistrationVerificationCodeSender verificationCodeSender,
    ISecretHashService secretHashService,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RequestEmailRebindVerificationCodeCommand, IResult<RequestEmailRebindVerificationCodeCommandResponse>> {

    public async Task<IResult<RequestEmailRebindVerificationCodeCommandResponse>> Handle(RequestEmailRebindVerificationCodeCommand request, CancellationToken cancellationToken) {
        var targetEmailAddressResult = EmailAddress.From(request.EmailAddress);
        if (targetEmailAddressResult.IsFailure) {
            return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(targetEmailAddressResult.Error);
        }

        var userResult = await userRepository.FindOneByIdAsync(request.UserId);
        if (userResult.IsFailure) return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(userResult.Error);

        var user = userResult.Value;
        if (!user.IsEmailVerified) {
            return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(DomainError.Auth.EmailAddressNotVerified);
        }

        var targetEmailAddress = targetEmailAddressResult.Value;
        if (user.EmailAddress.Equals(targetEmailAddress)) {
            return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(DomainError.Auth.EmailAddressUnchanged);
        }

        var existingEmailUserResult = await userRepository.FindOneByEmailAddressAsync(targetEmailAddress);
        if (existingEmailUserResult.IsSuccess && existingEmailUserResult.Value.Id != user.Id) {
            return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(DomainError.User.EmailAddressAlreadyExists);
        }

        if (existingEmailUserResult.IsFailure && existingEmailUserResult.Error != DomainError.User.NotFound) {
            return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(existingEmailUserResult.Error);
        }

        var utcNow = DateTime.UtcNow;

        EmailRebindVerification? verification = null;
        var verificationResult = await emailRebindVerificationRepository.FindOneByUserIdAsync(user.Id);
        if (verificationResult.IsSuccess) {
            verification = verificationResult.Value;
            var registerRequestResult = verification.RegisterRequest(utcNow);
            if (registerRequestResult.IsFailure) {
                return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(registerRequestResult.Error);
            }
        } else if (verificationResult.Error != DomainError.Auth.EmailRebindVerificationNotFound) {
            return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(verificationResult.Error);
        }

        var verificationCode = verificationCodeGenerator.Generate();
        var verificationCodeHash = secretHashService.Hash(verificationCode);
        var expiresAt = utcNow.AddMinutes(10);

        if (verification is null) {
            var createResult = EmailRebindVerification.From(new EmailRebindVerificationConstructorArgs {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                TargetEmailAddress = targetEmailAddress,
                VerificationCodeHash = verificationCodeHash,
                ExpiresAt = expiresAt,
                FailedAttemptCount = 0,
                RequestCount = 1,
                RequestCountDate = utcNow.Date,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
            });
            if (createResult.IsFailure) return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(createResult.Error);

            verification = createResult.Value;
            var insertResult = await emailRebindVerificationRepository.InsertOneAsync(verification);
            if (insertResult.IsFailure) return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(insertResult.Error);
        } else {
            var renewResult = verification.Renew(targetEmailAddress, verificationCodeHash, expiresAt, utcNow);
            if (renewResult.IsFailure) return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(renewResult.Error);

            var updateResult = await emailRebindVerificationRepository.UpdateOneAsync(verification);
            if (updateResult.IsFailure) return Result.Failure<RequestEmailRebindVerificationCodeCommandResponse>(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await verificationCodeSender.SendAsync(targetEmailAddress, verificationCode, VerificationCodePurpose.EmailRebind, cancellationToken);

        return Result.Success(new RequestEmailRebindVerificationCodeCommandResponse {
            EmailAddress = targetEmailAddress.Value,
            ExpiresAt = expiresAt,
        });
    }
}
