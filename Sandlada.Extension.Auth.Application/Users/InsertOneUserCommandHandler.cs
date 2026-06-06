using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed class InsertOneUserCommandHandler(
    IUserRepository userRepository,
    ISecretHashService secretHashService,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<InsertOneUserCommand, IResult<UserResponse>> {

    public async Task<IResult<UserResponse>> Handle(InsertOneUserCommand request, CancellationToken cancellationToken) {
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
            Id = Guid.NewGuid(),
            EmailAddress = emailAddressResult.Value,
            UniqueName = request.UniqueName,
            Role = roleResult.Value,
            PasswordHash = secretHashService.Hash(request.Password),
            IsEmailVerified = request.IsEmailVerified,
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
}
