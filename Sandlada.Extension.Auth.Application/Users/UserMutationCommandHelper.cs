using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Users;

internal static class UserMutationCommandHelper {
    public static IResult<EmailAddress> ResolveEmailAddress(string? emailAddress) {
        return EmailAddress.From(emailAddress ?? string.Empty);
    }

    public static IResult<UserRole> ResolveRole(string? role, UserRole defaultRole) {
        if (string.IsNullOrWhiteSpace(role)) {
            return Result.Success(defaultRole);
        }

        return UserRole.From(role);
    }

    public static async Task<IResult> EnsureEmailAddressAvailableAsync(IUserRepository userRepository, EmailAddress emailAddress, Guid? exceptUserId = null) {
        var existingUserResult = await userRepository.FindOneByEmailAddressAsync(emailAddress);
        return ResolveAvailabilityResult(existingUserResult, exceptUserId, DomainError.User.EmailAddressAlreadyExists);
    }

    public static async Task<IResult> EnsureUniqueNameAvailableAsync(IUserRepository userRepository, string? uniqueName, Guid? exceptUserId = null) {
        if (string.IsNullOrWhiteSpace(uniqueName)) {
            return Result.Success();
        }

        var existingUserResult = await userRepository.FindOneByUniqueNameAsync(uniqueName);
        return ResolveAvailabilityResult(existingUserResult, exceptUserId, DomainError.User.UniqueNameAlreadyExists);
    }

    public static async Task<IResult> ApplyUpdatesAsync(
        IUserRepository userRepository,
        ISecretHashService secretHashService,
        User user,
        string? emailAddress,
        string? uniqueName,
        string? role,
        string? password,
        bool? isEmailVerified,
        UserStatus? status = null
    ) {
        if (emailAddress is not null) {
            var emailAddressResult = ResolveEmailAddress(emailAddress);
            if (emailAddressResult.IsFailure) return emailAddressResult;

            var ensureEmailResult = await EnsureEmailAddressAvailableAsync(userRepository, emailAddressResult.Value, user.Id);
            if (ensureEmailResult.IsFailure) return ensureEmailResult;

            var updateEmailResult = user.UpdateEmailAddress(emailAddressResult.Value);
            if (updateEmailResult.IsFailure) return updateEmailResult;
        }

        if (uniqueName is not null) {
            var ensureUniqueNameResult = await EnsureUniqueNameAvailableAsync(userRepository, uniqueName, user.Id);
            if (ensureUniqueNameResult.IsFailure) return ensureUniqueNameResult;

            var updateUniqueNameResult = user.UpdateUniqueName(uniqueName);
            if (updateUniqueNameResult.IsFailure) return updateUniqueNameResult;
        }

        if (role is not null) {
            var roleResult = ResolveRole(role, user.Role);
            if (roleResult.IsFailure) return roleResult;

            var updateRoleResult = user.UpdateRole(roleResult.Value);
            if (updateRoleResult.IsFailure) return updateRoleResult;
        }

        if (password is not null) {
            if (string.IsNullOrWhiteSpace(password)) {
                return Result.Failure(DomainError.Auth.PasswordCannotBeEmpty);
            }

            var updatePasswordHashResult = user.UpdatePasswordHash(secretHashService.Hash(password));
            if (updatePasswordHashResult.IsFailure) return updatePasswordHashResult;
        }

        if (isEmailVerified.HasValue) {
            var updateEmailVerifiedResult = user.UpdateIsEmailVerified(isEmailVerified.Value);
            if (updateEmailVerifiedResult.IsFailure) return updateEmailVerifiedResult;
        }

        if (status is not null) {
            var updateStatusResult = user.UpdateStatus(status);
            if (updateStatusResult.IsFailure) return updateStatusResult;
        }

        return Result.Success();
    }

    private static IResult ResolveAvailabilityResult(IResult<Domain.Aggregates.User> existingUserResult, Guid? exceptUserId, DomainError alreadyExistsError) {
        if (existingUserResult.IsSuccess) {
            if (exceptUserId.HasValue && existingUserResult.Value.Id == exceptUserId.Value) {
                return Result.Success();
            }

            return Result.Failure(alreadyExistsError);
        }

        if (existingUserResult.Error == DomainError.User.NotFound) {
            return Result.Success();
        }

        return Result.Failure(existingUserResult.Error);
    }
}
