using HotChocolate;
using MediatR;
using Microsoft.AspNetCore.Http;
using Sandlada.Extension.Auth.Api.Endpoints;
using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Application.OAuthClient;
using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Application.Users;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Api.GraphQL;

/// <summary>
/// Root Query type for the Sandlada Auth GraphQL API.
/// Mirrors key read paths from the REST surface.
/// </summary>
public sealed class Query
{
    /// <summary>
    /// Returns the authenticated user (from cookie). Requires authentication.
    /// </summary>
    public async Task<UserResponse?> GetCurrentUser(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
        {
            // HotChocolate will turn this into an auth error via the Authorize attribute already
            return null;
        }

        var query = new FindOneUserByIdQuery(new FindOneUserByIdQueryArgs { UserId = userId });
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    /// <summary>
    /// Returns current user's profile. Auth required.
    /// </summary>
    public async Task<UserProfileResponse?> GetCurrentUserProfile(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            return null;

        var q = new FindOneUserProfileByUserIdQuery(new FindOneUserProfileByUserIdQueryArgs { UserId = userId });
        var result = await sender.Send(q, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    /// <summary>
    /// Admin: find any user by id.
    /// </summary>
    public async Task<UserResponse?> GetUserById(
        Guid userId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!IsAdministrator(httpContext)) return null;
        var query = new FindOneUserByIdQuery(new FindOneUserByIdQueryArgs { UserId = userId });
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    /// <summary>
    /// Admin: list OAuth clients.
    /// </summary>
    public async Task<List<FindOneOAuthClientByClientIdQueryResponse>> GetOAuthClients(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!IsAdministrator(httpContext)) return [];
        var query = new FindManyOAuthClientQuery();
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? result.Value : [];
    }

    /// <summary>
    /// Admin: get profile by user id.
    /// </summary>
    public async Task<UserProfileResponse?> GetUserProfileByUserId(
        Guid userId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!IsAdministrator(httpContext)) return null;
        var q = new FindOneUserProfileByUserIdQuery(new FindOneUserProfileByUserIdQueryArgs { UserId = userId });
        var result = await sender.Send(q, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    /// <summary>
    /// Current user status (lightweight). Auth required.
    /// </summary>
    public async Task<UserStatusResponse?> GetCurrentUserStatus(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            return null;

        var query = new FindOneCurrentUserUserStatusQuery(userId);
        var result = await sender.Send(query, cancellationToken);
        return result.IsSuccess ? result.Value : null;
    }

    private static bool IsAdministrator(HttpContext? ctx)
        => ctx?.User?.IsInRole(UserRole.AdministratorString) ?? false;
}
