using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;
using OpenIddict.Abstractions;

namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed class RemoveOneOAuthClientCommandHandler(
    IOAuthClientRepository oauthClientRepository,
    IOpenIddictApplicationManager applicationManager,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<RemoveOneOAuthClientCommand, IResult> {

    public async Task<IResult> Handle(RemoveOneOAuthClientCommand request, CancellationToken cancellationToken) {
        var result = await oauthClientRepository.FindOneByIdAsync(request.Id);
        if (result.IsFailure) {
            return Result.Failure(result.Error);
        }

        var client = result.Value;

        var openIddictApplication = await applicationManager.FindByClientIdAsync(client.ClientId, cancellationToken);
        if (openIddictApplication is not null) {
            await applicationManager.DeleteAsync(openIddictApplication, cancellationToken);
        }

        var removeResult = await oauthClientRepository.RemoveOneAsync(request.Id);
        if (removeResult.IsFailure) {
            return Result.Failure(removeResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}