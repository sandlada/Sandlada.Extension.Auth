using System.Security.Claims;
using MediatR;
using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Domain.Commons;
using Microsoft.AspNetCore.Mvc;

// Resolve ambiguity between Domain.IResult and AspNetCore.Http.IResult
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Sandlada.Extension.Auth.Api.Endpoints;

public static class UserProfileEndpoints {
    public static RouteGroupBuilder MapUserProfileEndpoints(this RouteGroupBuilder group) {
        // User self
        group.MapGet("/FindOneCurrentUserProfile", FindOneCurrentUserProfile)
            .RequireAuthorization()
            .WithName("FindOneCurrentUserProfile");

        group.MapPost("/InsertOneCurrentUserProfile", InsertOneCurrentUserProfile)
            .RequireAuthorization()
            .WithName("InsertOneCurrentUserProfile");

        group.MapPut("/UpdateOneCurrentUserProfile", UpdateOneCurrentUserProfile)
            .RequireAuthorization()
            .WithName("UpdateOneCurrentUserProfile");

        group.MapPut("/InsertOrUpdateOneCurrentUserProfile", InsertOrUpdateOneCurrentUserProfile)
            .RequireAuthorization()
            .WithName("InsertOrUpdateOneCurrentUserProfile");

        group.MapDelete("/RemoveOneCurrentUserProfile", RemoveOneCurrentUserProfile)
            .RequireAuthorization()
            .WithName("RemoveOneCurrentUserProfile");

        group.MapPost("/ResetOneCurrentUserProfile", ResetOneCurrentUserProfile)
            .RequireAuthorization()
            .WithName("ResetOneCurrentUserProfile");

        // Admin
        group.MapGet("/FindOneUserProfileByUserId/{userId:guid}", FindOneUserProfileByUserId)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("FindOneUserProfileByUserId");

        group.MapPost("/InsertOneUserProfileByUserId/{userId:guid}", InsertOneUserProfileByUserId)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("InsertOneUserProfileByUserId");

        group.MapPut("/UpdateOneUserProfileByUserId/{userId:guid}", UpdateOneUserProfileByUserId)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("UpdateOneUserProfileByUserId");

        group.MapPut("/InsertOrUpdateOneUserProfileByUserId/{userId:guid}", InsertOrUpdateOneUserProfileByUserId)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("InsertOrUpdateOneUserProfileByUserId");

        group.MapDelete("/RemoveOneUserProfileByUserId/{userId:guid}", RemoveOneUserProfileByUserId)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("RemoveOneUserProfileByUserId");

        group.MapPost("/ResetOneUserProfileByUserId/{userId:guid}", ResetOneUserProfileByUserId)
            .RequireAuthorization(policy => policy.RequireRole("Administrator"))
            .WithName("ResetOneUserProfileByUserId");

        return group;
    }

    private static async Task<IResult> FindOneCurrentUserProfile(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        if (!TryGetCurrentUserId(httpContext, out var userId)) return TypedResults.Unauthorized();
        var request = new FindOneUserProfileByUserIdQuery(new FindOneUserProfileByUserIdQueryArgs { UserId = userId });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> InsertOneCurrentUserProfile(
        [FromBody] InsertOneUserProfileCommandArgs requestArgs,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        if (!TryGetCurrentUserId(httpContext, out var userId)) return TypedResults.Unauthorized();
        var request = new InsertOneUserProfileCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> UpdateOneCurrentUserProfile(
        [FromBody] UpdateOneUserProfileCommandArgs requestArgs,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        if (!TryGetCurrentUserId(httpContext, out var userId)) return TypedResults.Unauthorized();
        var request = new UpdateOneUserProfileCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> InsertOrUpdateOneCurrentUserProfile(
        [FromBody] InsertOrUpdateOneUserProfileCommandArgs requestArgs,
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        if (!TryGetCurrentUserId(httpContext, out var userId)) return TypedResults.Unauthorized();
        var request = new InsertOrUpdateOneUserProfileCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> RemoveOneCurrentUserProfile(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        if (!TryGetCurrentUserId(httpContext, out var userId)) return TypedResults.Unauthorized();
        var request = new RemoveOneUserProfileCommand(new RemoveOneUserProfileCommandArgs { UserId = userId });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> ResetOneCurrentUserProfile(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        if (!TryGetCurrentUserId(httpContext, out var userId)) return TypedResults.Unauthorized();
        var request = new ResetOneUserProfileCommand(new ResetOneUserProfileCommandArgs { UserId = userId });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    // Admin endpoints
    private static async Task<IResult> FindOneUserProfileByUserId(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new FindOneUserProfileByUserIdQuery(new FindOneUserProfileByUserIdQueryArgs { UserId = userId });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> InsertOneUserProfileByUserId(
        Guid userId,
        [FromBody] InsertOneUserProfileCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new InsertOneUserProfileCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> UpdateOneUserProfileByUserId(
        Guid userId,
        [FromBody] UpdateOneUserProfileCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new UpdateOneUserProfileCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> InsertOrUpdateOneUserProfileByUserId(
        Guid userId,
        [FromBody] InsertOrUpdateOneUserProfileCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new InsertOrUpdateOneUserProfileCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> RemoveOneUserProfileByUserId(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new RemoveOneUserProfileCommand(new RemoveOneUserProfileCommandArgs { UserId = userId });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> ResetOneUserProfileByUserId(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new ResetOneUserProfileCommand(new ResetOneUserProfileCommandArgs { UserId = userId });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static bool TryGetCurrentUserId(HttpContext httpContext, out Guid userId) {
        var rawUserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(rawUserId, out userId);
    }

    private static IResult ToHttpResult<T>(Sandlada.Extension.Auth.Domain.Commons.IResult<T> result) {
        if (result.IsSuccess) return TypedResults.Ok(result.Value);
        return ToFailureResult<T>(result.Error);
    }

    private static IResult ToFailureResult<T>(DomainError error) {
        if (error == DomainError.UserProfile.NotFound) return TypedResults.NotFound(error);
        return TypedResults.BadRequest(error);
    }
}
