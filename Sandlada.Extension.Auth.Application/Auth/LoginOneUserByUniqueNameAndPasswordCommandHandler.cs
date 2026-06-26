using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Application.UserProfiles;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Auth;

public sealed class LoginOneUserByUniqueNameAndPasswordCommandHandler(
    IUserRepository userRepository,
    ISecretHashService secretHashService,
    IPasswordLoginAttemptRepository passwordLoginAttemptRepository,
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

        var emailAddress = user.EmailAddress;
        var utcNow = DateTime.UtcNow;

        // Find or create PasswordLoginAttempt
        var (attempt, isNewAttempt) = await GetOrCreateAttemptAsync(emailAddress, utcNow);

        // Check lockout
        if (attempt.IsLockedOut(utcNow)) return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.AccountLocked);

        // Register request (daily rate limit)
        var registerRequestResult = attempt.RegisterRequest(utcNow);
        if (registerRequestResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(registerRequestResult.Error);

        if (!user.IsEmailVerified) {
            await PersistAttemptAsync(attempt, isNewAttempt);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.EmailAddressNotVerified);
        }
        if (!secretHashService.Verify(request.Password, user.PasswordHash)) {
            // Track the failed attempt
            var failedResult = attempt.RegisterFailedAttempt(utcNow);
            await PersistAttemptAsync(attempt, isNewAttempt);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return failedResult.IsFailure
                ? Result.Failure<AuthenticatedUserResponse>(failedResult.Error)
                : Result.Failure<AuthenticatedUserResponse>(DomainError.Auth.InvalidCredentials);
        }

        // Success — reset attempt tracking
        attempt.Reset(utcNow);
        await PersistAttemptAsync(attempt, isNewAttempt);

        var initializeFirstLoginResult = await firstLoginUserProfileInitializer.InitializeAsync(user, cancellationToken);
        if (initializeFirstLoginResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(initializeFirstLoginResult.Error);

        if (initializeFirstLoginResult.Value) {
            var updateUserResult = await userRepository.UpdateOneAsync(user);
            if (updateUserResult.IsFailure) return Result.Failure<AuthenticatedUserResponse>(updateUserResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(AuthenticatedUserResponse.From(user));
    }

    private async Task<(PasswordLoginAttempt Attempt, bool IsNew)> GetOrCreateAttemptAsync(EmailAddress emailAddress, DateTime utcNow) {
        var attemptResult = await passwordLoginAttemptRepository.FindOneByEmailAddressAsync(emailAddress);
        if (attemptResult.IsSuccess) return (attemptResult.Value, false);

        var createResult = PasswordLoginAttempt.CreateNew(emailAddress, utcNow);
        return (createResult.Value, true);
    }

    private async Task PersistAttemptAsync(PasswordLoginAttempt attempt, bool isNew) {
        if (isNew) {
            await passwordLoginAttemptRepository.InsertOneAsync(attempt);
        } else {
            await passwordLoginAttemptRepository.UpdateOneAsync(attempt);
        }
    }
}
