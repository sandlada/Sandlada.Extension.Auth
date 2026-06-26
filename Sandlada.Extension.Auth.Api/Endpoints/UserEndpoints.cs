using MediatR;
using Sandlada.Extension.Auth.Application.Users;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

// Resolve ambiguity between Domain.IResult and AspNetCore.Http.IResult
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Sandlada.Extension.Auth.Api.Endpoints;

public static class UserEndpoints {
    public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group) {
        // Admin endpoints
        group.MapGet("/FindOneUserById/{userId:guid}", FindOneUserById)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("FindOneUserById");

        group.MapPut("/UpdateOneUserEmailVerified/{userId:guid}", UpdateOneUserEmailVerified)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("UpdateOneUserEmailVerified");

        group.MapPut("/UpdateOneUser/{userId:guid}", UpdateOneUser)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("UpdateOneUser");

        group.MapPost("/InsertOneUser", InsertOneUser)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("InsertOneUser");

        group.MapPut("/InsertOrUpdateOneUser", InsertOrUpdateOneUser)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("InsertOrUpdateOneUser");

        group.MapDelete("/RemoveOneUser/{userId:guid}", RemoveOneUser)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("RemoveOneUser");

        // Admin: UserStatus
        group.MapPut("/UpdateOneUserUserStatus/{userId:guid}", UpdateOneUserUserStatus)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("UpdateOneUserUserStatus");

        // User self: UserStatus
        group.MapGet("/FindOneCurrentUserStatus", FindOneCurrentUserStatus)
            .RequireAuthorization()
            .WithName("FindOneCurrentUserStatus");

        return group;
    }

    private static async Task<IResult> FindOneUserById(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new FindOneUserByIdQuery(new FindOneUserByIdQueryArgs { UserId = userId });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> UpdateOneUserEmailVerified(
        Guid userId,
        [FromBody] UpdateOneUserEmailVerifiedCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new UpdateOneUserEmailVerifiedCommand(new UpdateOneUserEmailVerifiedCommandArgs {
            UserId = userId,
            IsEmailVerified = requestArgs.IsEmailVerified,
        });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> UpdateOneUser(
        Guid userId,
        [FromBody] UpdateOneUserCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new UpdateOneUserCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> InsertOneUser(
        [FromBody] InsertOneUserCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new InsertOneUserCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> InsertOrUpdateOneUser(
        [FromBody] InsertOrUpdateOneUserCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new InsertOrUpdateOneUserCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<IResult> RemoveOneUser(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new RemoveOneUserCommand(new RemoveOneUserCommandArgs { UserId = userId });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    // UserStatus: Admin
    private static async Task<IResult> UpdateOneUserUserStatus(
        Guid userId,
        [FromBody] UpdateOneUserUserStatusCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new UpdateOneUserUserStatusCommand(userId, requestArgs.Status);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    // UserStatus: Self
    private static async Task<IResult> FindOneCurrentUserStatus(
        HttpContext httpContext,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId)) {
            return TypedResults.Unauthorized();
        }

        var request = new FindOneCurrentUserUserStatusQuery(userId);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static IResult ToHttpResult<T>(Sandlada.Extension.Auth.Domain.Commons.IResult<T> result) {
        if (result.IsSuccess) return TypedResults.Ok(result.Value);
        return ToFailureResult<T>(result.Error);
    }

    private static IResult ToFailureResult<T>(DomainError error) {
        if (error == DomainError.User.NotFound) return TypedResults.NotFound(error);
        return TypedResults.BadRequest(error);
    }
}
