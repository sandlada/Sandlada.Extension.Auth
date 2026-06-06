using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class RequestRegistrationVerificationCodeCommandHandler(
    IUserRepository userRepository,
    IRegistrationVerificationRepository registrationVerificationRepository,
    IRegistrationVerificationCodeGenerator registrationVerificationCodeGenerator,
    IRegistrationVerificationCodeSender registrationVerificationCodeSender,
    ISecretHashService secretHashService,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RequestRegistrationVerificationCodeCommand, IResult<RequestRegistrationVerificationCodeCommandResponse>> {

    public async Task<IResult<RequestRegistrationVerificationCodeCommandResponse>> Handle(RequestRegistrationVerificationCodeCommand request, CancellationToken cancellationToken) {
        var emailAddressResult = EmailAddress.From(request.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(emailAddressResult.Error);
        if (string.IsNullOrWhiteSpace(request.Password)) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(DomainError.Auth.PasswordCannotBeEmpty);

        var emailAddress = emailAddressResult.Value;
        var passwordHash = secretHashService.Hash(request.Password);
        var utcNow = DateTime.UtcNow;

        User? existingUser = null;
        var existingUserResult = await userRepository.FindOneByEmailAddressAsync(emailAddress);
        if (existingUserResult.IsSuccess) {
            existingUser = existingUserResult.Value;
            if (existingUser.IsEmailVerified) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(DomainError.User.EmailAddressAlreadyExists);
        } else if (existingUserResult.Error != DomainError.User.NotFound) {
            return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(existingUserResult.Error);
        }

        var verificationResult = await registrationVerificationRepository.FindOneByEmailAddressAsync(emailAddress);
        RegistrationVerification? verification = null;
        if (verificationResult.IsSuccess) {
            verification = verificationResult.Value;
            var registerRequestResult = verification.RegisterRequest(utcNow);
            if (registerRequestResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(registerRequestResult.Error);
        } else if (verificationResult.Error != DomainError.Auth.VerificationCodeNotFound) {
            return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(verificationResult.Error);
        }

        var verificationCode = registrationVerificationCodeGenerator.Generate();
        var verificationCodeHash = secretHashService.Hash(verificationCode);
        var expiresAt = utcNow.AddMinutes(10);

        if (existingUser is null) {
            var createUserResult = User.From(new UserConstructorArgs {
                Id = Guid.NewGuid(),
                EmailAddress = emailAddress,
                UniqueName = null,

                Role = UserRole.Normal,
                PasswordHash = passwordHash,
                IsEmailVerified = false,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
            });
            if (createUserResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(createUserResult.Error);

            existingUser = createUserResult.Value;
            var insertUserResult = await userRepository.InsertOneAsync(existingUser);
            if (insertUserResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(insertUserResult.Error);
        } else {
            var updatePasswordResult = existingUser.UpdatePasswordHash(passwordHash);
            if (updatePasswordResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(updatePasswordResult.Error);

            var markUnverifiedResult = existingUser.UpdateIsEmailVerified(false);
            if (markUnverifiedResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(markUnverifiedResult.Error);

            var repositoryUpdateResult = await userRepository.UpdateOneAsync(existingUser);
            if (repositoryUpdateResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(repositoryUpdateResult.Error);
        }

        if (verification is null) {
            var createResult = RegistrationVerification.From(new RegistrationVerificationConstructorArgs {
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
            if (createResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(createResult.Error);

            verification = createResult.Value;
            var insertResult = await registrationVerificationRepository.InsertOneAsync(verification);
            if (insertResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(insertResult.Error);
        } else {
            var renewResult = verification.Renew(verificationCodeHash, expiresAt, utcNow);
            if (renewResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(renewResult.Error);

            var updateResult = await registrationVerificationRepository.UpdateOneAsync(verification);
            if (updateResult.IsFailure) return Result.Failure<RequestRegistrationVerificationCodeCommandResponse>(updateResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await registrationVerificationCodeSender.SendAsync(emailAddress, verificationCode, VerificationCodePurpose.Registration, cancellationToken);

        return Result.Success(new RequestRegistrationVerificationCodeCommandResponse {
            EmailAddress = emailAddress.Value,
            ExpiresAt = expiresAt,
        });
    }
}
