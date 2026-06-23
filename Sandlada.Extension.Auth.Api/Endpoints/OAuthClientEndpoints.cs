using MediatR;
using Microsoft.AspNetCore.Mvc;
using Sandlada.Extension.Auth.Application.OAuthClient;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Sandlada.Extension.Auth.Api.Endpoints;

public static class OAuthClientEndpoints {
    public static RouteGroupBuilder MapOAuthClientEndpoints(this RouteGroupBuilder group) {
        group.MapPost("/InsertOne", InsertOne)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("InsertOneOAuthClient");

        group.MapGet("/FindOneByClientId/{clientId}", FindOneByClientId)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("FindOneOAuthClientByClientId");

        group.MapGet("/FindMany", FindMany)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("FindManyOAuthClients");

        group.MapPost("/RemoveOne/{id:guid}", RemoveOne)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("RemoveOneOAuthClient");

        return group;
    }

    private static async Task<IResult> InsertOne(
        [FromBody] InsertOneOAuthClientCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new InsertOneOAuthClientCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> FindOneByClientId(
        string clientId,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new FindOneOAuthClientByClientIdQuery(new FindOneOAuthClientByClientIdQueryArgs {
            ClientId = clientId,
        });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> FindMany(
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new FindManyOAuthClientQuery();
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> RemoveOne(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new RemoveOneOAuthClientCommand(new RemoveOneOAuthClientCommandArgs {
            Id = id,
        });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static IResult ToHttpResult(Sandlada.Extension.Auth.Domain.Commons.IResult result) {
        if (result.IsSuccess) return TypedResults.Ok();
        return ToFailureResult(result.Error);
    }

    private static IResult ToHttpResult<T>(Sandlada.Extension.Auth.Domain.Commons.IResult<T> result) {
        if (result.IsSuccess) return TypedResults.Ok(result.Value);
        return ToFailureResult<T>(result.Error);
    }

    private static IResult ToFailureResult(DomainError error) {
        if (error == DomainError.OAuthClient.NotFound) return TypedResults.NotFound(error);
        return TypedResults.BadRequest(error);
    }

    private static IResult ToFailureResult<T>(DomainError error) {
        if (error == DomainError.OAuthClient.NotFound) return TypedResults.NotFound(error);
        return TypedResults.BadRequest(error);
    }
}