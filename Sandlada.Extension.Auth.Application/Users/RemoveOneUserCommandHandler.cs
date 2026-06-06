using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed class RemoveOneUserCommandHandler(
    IUserRepository userRepository,
    IAuthSessionRepository authSessionRepository,
    IEmailRebindVerificationRepository emailRebindVerificationRepository,
    IRegistrationVerificationRepository registrationVerificationRepository,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RemoveOneUserCommand, IResult<RemoveOneUserCommandResponse>> {

    public async Task<IResult<RemoveOneUserCommandResponse>> Handle(RemoveOneUserCommand request, CancellationToken cancellationToken) {
        var userResult = await userRepository.FindOneByIdAsync(request.UserId);
        if (userResult.IsFailure) return Result.Failure<RemoveOneUserCommandResponse>(userResult.Error);

        var user = userResult.Value;

        var removeSessionsResult = await authSessionRepository.RemoveManyByUserIdAsync(user.Id);
        if (removeSessionsResult.IsFailure) return Result.Failure<RemoveOneUserCommandResponse>(removeSessionsResult.Error);

        var emailRebindVerificationResult = await emailRebindVerificationRepository.FindOneByUserIdAsync(user.Id);
        if (emailRebindVerificationResult.IsSuccess) {
            var removeEmailRebindVerificationResult = await emailRebindVerificationRepository.RemoveOneAsync(emailRebindVerificationResult.Value.Id);
            if (removeEmailRebindVerificationResult.IsFailure) return Result.Failure<RemoveOneUserCommandResponse>(removeEmailRebindVerificationResult.Error);
        } else if (emailRebindVerificationResult.Error != DomainError.Auth.EmailRebindVerificationNotFound) {
            return Result.Failure<RemoveOneUserCommandResponse>(emailRebindVerificationResult.Error);
        }

        var registrationVerificationResult = await registrationVerificationRepository.FindOneByEmailAddressAsync(user.EmailAddress);
        if (registrationVerificationResult.IsSuccess) {
            var removeRegistrationVerificationResult = await registrationVerificationRepository.RemoveOneAsync(registrationVerificationResult.Value.Id);
            if (removeRegistrationVerificationResult.IsFailure) return Result.Failure<RemoveOneUserCommandResponse>(removeRegistrationVerificationResult.Error);
        } else if (registrationVerificationResult.Error != DomainError.Auth.VerificationCodeNotFound) {
            return Result.Failure<RemoveOneUserCommandResponse>(registrationVerificationResult.Error);
        }

        var removeUserResult = await userRepository.RemoveOneAsync(user.Id);
        if (removeUserResult.IsFailure) return Result.Failure<RemoveOneUserCommandResponse>(removeUserResult.Error);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return Result.Success(new RemoveOneUserCommandResponse {
            Removed = true,
        });
    }
}
