using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed class InsertOrUpdateOneUserCommandHandler(
    IUserRepository userRepository,
    ISecretHashService secretHashService,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<InsertOrUpdateOneUserCommand, IResult<UserResponse>> {

    public async Task<IResult<UserResponse>> Handle(InsertOrUpdateOneUserCommand request, CancellationToken cancellationToken) {
        var resolveExistingUserResult = await this.ResolveExistingUserAsync(request);
        if (resolveExistingUserResult.IsFailure) return Result.Failure<UserResponse>(resolveExistingUserResult.Error);

        if (resolveExistingUserResult.Value is null) {
            return await this.InsertOneAsync(request, cancellationToken);
        }

        return await this.UpdateOneAsync(resolveExistingUserResult.Value, request, cancellationToken);
    }

    private async Task<IResult<UserResponse>> InsertOneAsync(InsertOrUpdateOneUserCommand request, CancellationToken cancellationToken) {
        var emailAddressResult = UserMutationCommandHelper.ResolveEmailAddress(request.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<UserResponse>(emailAddressResult.Error);

        var roleResult = UserMutationCommandHelper.ResolveRole(request.Role, UserRole.Normal);
        if (roleResult.IsFailure) return Result.Failure<UserResponse>(roleResult.Error);

        if (string.IsNullOrWhiteSpace(request.Password)) {
            return Result.Failure<UserResponse>(DomainError.Auth.PasswordCannotBeEmpty);
        }

        var ensureEmailResult = await UserMutationCommandHelper.EnsureEmailAddressAvailableAsync(userRepository, emailAddressResult.Value);
        if (ensureEmailResult.IsFailure) return Result.Failure<UserResponse>(ensureEmailResult.Error);

        var ensureUniqueNameResult = await UserMutationCommandHelper.EnsureUniqueNameAvailableAsync(userRepository, request.UniqueName);
        if (ensureUniqueNameResult.IsFailure) return Result.Failure<UserResponse>(ensureUniqueNameResult.Error);

        var statusResult = request.Status is not null
            ? UserStatus.From(new UserStatusConstructorArgs { Code = request.Status })
            : Result.Success(UserStatus.Enabled);

        if (statusResult.IsFailure) return Result.Failure<UserResponse>(statusResult.Error);

        var utcNow = DateTime.UtcNow;
        var createUserResult = User.From(new UserConstructorArgs {
            Id = request.UserId ?? Guid.NewGuid(),
            EmailAddress = emailAddressResult.Value,
            UniqueName = request.UniqueName,
            Role = roleResult.Value,
            PasswordHash = secretHashService.Hash(request.Password),
            IsEmailVerified = request.IsEmailVerified ?? false,
            FirstLoginAt = null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            Status = statusResult.Value,
        });
        if (createUserResult.IsFailure) return Result.Failure<UserResponse>(createUserResult.Error);

        var insertUserResult = await userRepository.InsertOneAsync(createUserResult.Value);
        if (insertUserResult.IsFailure) return Result.Failure<UserResponse>(insertUserResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(UserResponse.From(createUserResult.Value));
    }

    private async Task<IResult<UserResponse>> UpdateOneAsync(User user, InsertOrUpdateOneUserCommand request, CancellationToken cancellationToken) {
        UserStatus? status = null;
        if (request.Status is not null) {
            var statusResult = UserStatus.From(new UserStatusConstructorArgs { Code = request.Status });
            if (statusResult.IsFailure) return Result.Failure<UserResponse>(statusResult.Error);
            status = statusResult.Value;
        }

        var applyUpdatesResult = await UserMutationCommandHelper.ApplyUpdatesAsync(
            userRepository,
            secretHashService,
            user,
            request.EmailAddress,
            request.UniqueName,
            request.Role,
            request.Password,
            request.IsEmailVerified,
            status
        );
        if (applyUpdatesResult.IsFailure) return Result.Failure<UserResponse>(applyUpdatesResult.Error);

        var updateUserResult = await userRepository.UpdateOneAsync(user);
        if (updateUserResult.IsFailure) return Result.Failure<UserResponse>(updateUserResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(UserResponse.From(user));
    }

    private async Task<IResult<User?>> ResolveExistingUserAsync(InsertOrUpdateOneUserCommand request) {
        if (request.UserId.HasValue) {
            var userResult = await userRepository.FindOneByIdAsync(request.UserId.Value);
            if (userResult.IsSuccess) return Result.Success<User?>(userResult.Value);
            if (userResult.Error != DomainError.User.NotFound) return Result.Failure<User?>(userResult.Error);
            return Result.Success<User?>(null);
        }

        if (string.IsNullOrWhiteSpace(request.EmailAddress)) {
            return Result.Failure<User?>(DomainError.User.UpsertKeyRequired);
        }

        var emailAddressResult = UserMutationCommandHelper.ResolveEmailAddress(request.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<User?>(emailAddressResult.Error);

        var userByEmailResult = await userRepository.FindOneByEmailAddressAsync(emailAddressResult.Value);
        if (userByEmailResult.IsSuccess) return Result.Success<User?>(userByEmailResult.Value);
        if (userByEmailResult.Error != DomainError.User.NotFound) return Result.Failure<User?>(userByEmailResult.Error);

        return Result.Success<User?>(null);
    }
}
