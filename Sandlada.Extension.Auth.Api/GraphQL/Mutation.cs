using HotChocolate;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Sandlada.Extension.Auth.Api.Endpoints;
using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Application.Users;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Api.GraphQL;

/// <summary>
/// Root Mutation type.
/// Delegates to the same Application layer MediatR commands used by REST.
/// </summary>
public sealed class Mutation
{
    /// <summary>
    /// Request registration verification code (anonymous).
    /// </summary>
    public async Task<RequestRegistrationVerificationCodeCommandResponse?> RequestRegistrationVerificationCode(
        RequestRegistrationVerificationCodeCommandArgs input,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var cmd = new RequestRegistrationVerificationCodeCommand(input);
        var result = await sender.Send(cmd, cancellationToken);
        return result.IsSuccess ? result.Value : throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Register a new user (anonymous). On success performs cookie sign-in.
    /// </summary>
    public async Task<AuthenticatedUserResponse?> Register(
        RegisterOneUserCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cmd = new RegisterOneUserCommand(input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess)
        {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
            return result.Value;
        }
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Login by email + password (anonymous). Performs cookie sign-in on success.
    /// </summary>
    public async Task<AuthenticatedUserResponse?> LoginByEmailAddressAndPassword(
        LoginOneUserByEmailAddressAndPasswordCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cmd = new LoginOneUserByEmailAddressAndPasswordCommand(input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess)
        {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
            return result.Value;
        }
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Login by unique name + password.
    /// </summary>
    public async Task<AuthenticatedUserResponse?> LoginByUniqueNameAndPassword(
        LoginOneUserByUniqueNameAndPasswordCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cmd = new LoginOneUserByUniqueNameAndPasswordCommand(input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess)
        {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
            return result.Value;
        }
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Login by email + verification code (anonymous). Performs cookie sign-in on success.
    /// </summary>
    public async Task<AuthenticatedUserResponse?> LoginByEmailAddressAndVerificationCode(
        LoginOneUserByEmailAddressAndVerificationCodeCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cmd = new LoginOneUserByEmailAddressAndVerificationCodeCommand(input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess)
        {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
            return result.Value;
        }
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Login by unique name + verification code (anonymous). Performs cookie sign-in on success.
    /// </summary>
    public async Task<AuthenticatedUserResponse?> LoginByUniqueNameAndVerificationCode(
        LoginOneUserByUniqueNameAndVerificationCodeCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var cmd = new LoginOneUserByUniqueNameAndVerificationCodeCommand(input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess)
        {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
            return result.Value;
        }
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Request a login verification code (anon).
    /// </summary>
    public async Task<RequestLoginVerificationCodeCommandResponse?> RequestLoginVerificationCode(
        RequestLoginVerificationCodeCommandArgs input,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var cmd = new RequestLoginVerificationCodeCommand(input);
        var result = await sender.Send(cmd, cancellationToken);
        return result.IsSuccess ? result.Value : throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Logout current session (auth required).
    /// </summary>
    public async Task<bool> Logout(
        ISender sender,
        HttpContext httpContext)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out _))
            throw new GraphQLException("Unauthorized");

        var cmd = new RemoveOneUserSessionCommand(new RemoveOneUserSessionCommandArgs
        {
            SessionId = httpContext.Connection.Id
        });
        await sender.Send(cmd, CancellationToken.None);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return true;
    }

    /// <summary>
    /// Logout all sessions for current user (auth).
    /// </summary>
    public async Task<bool> LogoutAllSessions(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            throw new GraphQLException("Unauthorized");

        var cmd = new RemoveManyUserSessionsByUserIdCommand(new RemoveManyUserSessionsByUserIdCommandArgs { UserId = userId });
        await sender.Send(cmd, cancellationToken);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return true;
    }

    /// <summary>
    /// Request email rebind verification code (auth required).
    /// </summary>
    public async Task<RequestEmailRebindVerificationCodeCommandResponse?> RequestEmailRebindVerificationCode(
        RequestEmailRebindVerificationCodeCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            throw new GraphQLException("Unauthorized");

        var cmd = new RequestEmailRebindVerificationCodeCommand(userId, input);
        var result = await sender.Send(cmd, cancellationToken);
        return result.IsSuccess ? result.Value : throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Confirm email rebind (auth required). Performs cookie sign-in on success.
    /// </summary>
    public async Task<AuthenticatedUserResponse?> ConfirmEmailRebind(
        ConfirmEmailRebindCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            throw new GraphQLException("Unauthorized");

        var cmd = new ConfirmEmailRebindCommand(userId, input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess)
        {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
            return result.Value;
        }
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Insert current user profile (auth).
    /// </summary>
    public async Task<UserProfileResponse?> InsertCurrentUserProfile(
        InsertOneUserProfileCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            throw new GraphQLException("Unauthorized");

        var cmd = new InsertOneUserProfileCommand(userId, input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess) return result.Value;
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Update current user profile (auth).
    /// </summary>
    public async Task<UserProfileResponse?> UpdateCurrentUserProfile(
        UpdateOneUserProfileCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            throw new GraphQLException("Unauthorized");

        var cmd = new UpdateOneUserProfileCommand(userId, input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess) return result.Value;
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Insert or update current user profile (auth). Upserts the profile.
    /// </summary>
    public async Task<UserProfileResponse?> InsertOrUpdateCurrentUserProfile(
        InsertOrUpdateOneUserProfileCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            throw new GraphQLException("Unauthorized");

        var cmd = new InsertOrUpdateOneUserProfileCommand(userId, input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess) return result.Value;
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Reset current user profile to defaults (auth).
    /// </summary>
    public async Task<UserProfileResponse?> ResetOneCurrentUserProfile(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId))
            throw new GraphQLException("Unauthorized");

        var cmd = new ResetOneUserProfileCommand(new ResetOneUserProfileCommandArgs { UserId = userId });
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess) return result.Value;
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Admin only: insert a new user.
    /// </summary>
    public async Task<UserResponse?> InsertOneUser(
        InsertOneUserCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUserAdministrator(httpContext))
            throw new GraphQLException("Administrator role required");

        var cmd = new InsertOneUserCommand(input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess) return result.Value;
        throw GraphQLError(result.Error);
    }

    /// <summary>
    /// Admin only: insert profile for a user.
    /// </summary>
    public async Task<UserProfileResponse?> InsertUserProfileByUserId(
        Guid userId,
        InsertOneUserProfileCommandArgs input,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (!IsCurrentUserAdministrator(httpContext))
            throw new GraphQLException("Administrator role required");

        var cmd = new InsertOneUserProfileCommand(userId, input);
        var result = await sender.Send(cmd, cancellationToken);
        if (result.IsSuccess) return result.Value;
        throw GraphQLError(result.Error);
    }

    private static bool IsCurrentUserAdministrator(HttpContext? ctx)
    {
        return ctx?.User?.IsInRole(UserRole.AdministratorString) == true;
    }

    private static GraphQLException GraphQLError(DomainError error)
    {
        return new GraphQLException(
            ErrorBuilder.New()
                .SetMessage(error.ToString())
                .SetCode(error.Code)
                .Build());
    }
}
