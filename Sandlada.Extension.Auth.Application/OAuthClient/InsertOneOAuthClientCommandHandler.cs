using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using MediatR;
using OpenIddict.Abstractions;

namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed class InsertOneOAuthClientCommandHandler(
    IOAuthClientRepository oauthClientRepository,
    IOpenIddictApplicationManager applicationManager,
    IApplicationUnitOfWork unitOfWork
) : IRequestHandler<InsertOneOAuthClientCommand, IResult<InsertOneOAuthClientCommandResponse>> {

    public async Task<IResult<InsertOneOAuthClientCommandResponse>> Handle(InsertOneOAuthClientCommand request, CancellationToken cancellationToken) {
        var existingResult = await oauthClientRepository.FindOneByClientIdAsync(request.ClientId);
        if (existingResult.IsSuccess) {
            return Result.Failure<InsertOneOAuthClientCommandResponse>(DomainError.OAuthClient.ClientIdAlreadyExists);
        }

        var utcNow = DateTime.UtcNow;
        var clientId = Guid.NewGuid();

        var clientResult = Domain.Aggregates.OAuthClient.From(new OAuthClientConstructorArgs {
            Id = clientId,
            ClientId = request.ClientId,
            DisplayName = request.DisplayName,
            RedirectUris = request.RedirectUris,
            PostLogoutRedirectUris = request.PostLogoutRedirectUris,
            AllowedScopes = request.AllowedScopes.Count == 0
                ? ["openid", "profile", "email", "offline_access"]
                : request.AllowedScopes,
            AllowedGrantTypes = request.AllowedGrantTypes.Count == 0
                ? ["authorization_code", "refresh_token"]
                : request.AllowedGrantTypes,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        });

        if (clientResult.IsFailure) {
            return Result.Failure<InsertOneOAuthClientCommandResponse>(clientResult.Error);
        }

        var client = clientResult.Value;

        var clientSecret = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");

        var openIddictDescriptor = new OpenIddictApplicationDescriptor {
            ClientId = client.ClientId,
            DisplayName = client.DisplayName,
            ClientSecret = clientSecret,
        };

        foreach (var uri in client.RedirectUris) {
            openIddictDescriptor.RedirectUris.Add(new Uri(uri));
        }

        foreach (var uri in client.PostLogoutRedirectUris) {
            openIddictDescriptor.PostLogoutRedirectUris.Add(new Uri(uri));
        }

        foreach (var scope in client.AllowedScopes) {
            openIddictDescriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.Scope + scope);
        }

        foreach (var grantType in client.AllowedGrantTypes) {
            openIddictDescriptor.Permissions.Add(OpenIddictConstants.Permissions.Prefixes.GrantType + grantType);
        }

        await applicationManager.CreateAsync(openIddictDescriptor, cancellationToken);

        var insertResult = await oauthClientRepository.InsertOneAsync(client);
        if (insertResult.IsFailure) {
            return Result.Failure<InsertOneOAuthClientCommandResponse>(insertResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(InsertOneOAuthClientCommandResponse.From(client, clientSecret));
    }
}