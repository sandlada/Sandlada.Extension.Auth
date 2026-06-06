using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class RequestLoginVerificationCodeCommandHandler(
    IUserRepository userRepository,
    ILoginVerificationRepository loginVerificationRepository,
    IRegistrationVerificationCodeGenerator registrationVerificationCodeGenerator,
    IRegistrationVerificationCodeSender registrationVerificationCodeSender,
    ISecretHashService secretHashService,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RequestLoginVerificationCodeCommand, IResult<RequestLoginVerificationCodeCommandResponse>> {

    public async Task<IResult<RequestLoginVerificationCodeCommandResponse>> Handle(RequestLoginVerificationCodeCommand request, CancellationToken cancellationToken) {
        if (!string.IsNullOrWhiteSpace(request.EmailAddress) && !string.IsNullOrWhiteSpace(request.UniqueName)) {
            return Result.Failure<RequestLoginVerificationCodeCommandResponse>(DomainError.Auth.InvalidCredentials);
        }

        if (string.IsNullOrWhiteSpace(request.EmailAddress) && string.IsNullOrWhiteSpace(request.UniqueName)) {
            return Result.Failure<RequestLoginVerificationCodeCommandResponse>(DomainError.Auth.InvalidCredentials);
        }

        EmailAddress emailAddress;
        User user;

        if (!string.IsNullOrWhiteSpace(request.EmailAddress)) {
            var emailAddressResult = EmailAddress.From(request.EmailAddress);
            if (emailAddressResult.IsFailure) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(emailAddressResult.Error);

            emailAddress = emailAddressResult.Value;

            var userResult = await userRepository.FindOneByEmailAddressAsync(emailAddress);
            if (userResult.IsFailure) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(DomainError.Auth.InvalidCredentials);

            user = userResult.Value;
        } else {
            var userResult = await userRepository.FindOneByUniqueNameAsync(request.UniqueName!);
            if (userResult.IsFailure) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(DomainError.Auth.InvalidCredentials);

            user = userResult.Value;
            emailAddress = user.EmailAddress;
        }

        if (!user.IsEmailVerified) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(DomainError.Auth.EmailAddressNotVerified);

        var utcNow = DateTime.UtcNow;
        var verificationResult = await loginVerificationRepository.FindOneByEmailAddressAsync(emailAddress);
        LoginVerification? verification = null;
        if (verificationResult.IsSuccess) {
            verification = verificationResult.Value;
            var registerRequestResult = verification.RegisterRequest(utcNow);
            if (registerRequestResult.IsFailure) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(registerRequestResult.Error);
        } else if (verificationResult.Error != DomainError.Auth.VerificationCodeNotFound) {
            return Result.Failure<RequestLoginVerificationCodeCommandResponse>(verificationResult.Error);
        }

        var verificationCode = registrationVerificationCodeGenerator.Generate();
        var verificationCodeHash = secretHashService.Hash(verificationCode);
        var expiresAt = utcNow.AddMinutes(10);

        if (verification is null) {
            var createResult = LoginVerification.From(new LoginVerificationConstructorArgs {
                Id = Guid.NewGuid(),
                EmailAddress = emailAddress,
                VerificationCodeHash = verificationCodeHash,
                ExpiresAt = expiresAt,
                FailedAttemptCount = 0,
                RequestCount = 1,
                RequestCountDate = utcNow.Date,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
            });
            if (createResult.IsFailure) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(createResult.Error);

            verification = createResult.Value;
            var insertResult = await loginVerificationRepository.InsertOneAsync(verification);
            if (insertResult.IsFailure) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(insertResult.Error);
        } else {
            var renewResult = verification.Renew(verificationCodeHash, expiresAt, utcNow);
            if (renewResult.IsFailure) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(renewResult.Error);

            var updateResult = await loginVerificationRepository.UpdateOneAsync(verification);
            if (updateResult.IsFailure) return Result.Failure<RequestLoginVerificationCodeCommandResponse>(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await registrationVerificationCodeSender.SendAsync(emailAddress, verificationCode, VerificationCodePurpose.Login, cancellationToken);

        return Result.Success(new RequestLoginVerificationCodeCommandResponse {
            EmailAddress = emailAddress.Value,
            ExpiresAt = expiresAt,
        });
    }
}