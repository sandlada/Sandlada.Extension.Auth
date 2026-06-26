using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

// Resolve ambiguity between Domain.IResult and AspNetCore.Http.IResult
using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Sandlada.Extension.Auth.Api.Endpoints;

public static class AuthEndpoints {
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group) {
        group.MapPost("/RequestRegistrationVerificationCode", RequestRegistrationVerificationCode)
            .AllowAnonymous()
            .WithName("RequestRegistrationVerificationCode");

        group.MapPost("/Register", Register)
            .AllowAnonymous()
            .WithName("Register");

        group.MapPost("/RequestEmailRebindVerificationCode", RequestEmailRebindVerificationCode)
            .RequireAuthorization()
            .WithName("RequestEmailRebindVerificationCode");

        group.MapPost("/ConfirmEmailRebind", ConfirmEmailRebind)
            .RequireAuthorization()
            .WithName("ConfirmEmailRebind");

        group.MapPost("/LoginByEmailAddressAndPassword", LoginByEmailAddressAndPassword)
            .AllowAnonymous()
            .WithName("LoginByEmailAddressAndPassword");

        group.MapPost("/LoginByUniqueNameAndPassword", LoginByUniqueNameAndPassword)
            .AllowAnonymous()
            .WithName("LoginByUniqueNameAndPassword");

        group.MapPost("/RequestLoginVerificationCode", RequestLoginVerificationCode)
            .AllowAnonymous()
            .WithName("RequestLoginVerificationCode");

        group.MapPost("/LoginByEmailAddressAndVerificationCode", LoginByEmailAddressAndVerificationCode)
            .AllowAnonymous()
            .WithName("LoginByEmailAddressAndVerificationCode");

        group.MapPost("/LoginByUniqueNameAndVerificationCode", LoginByUniqueNameAndVerificationCode)
            .AllowAnonymous()
            .WithName("LoginByUniqueNameAndVerificationCode");

        group.MapPost("/Logout", Logout)
            .RequireAuthorization()
            .WithName("Logout");

        group.MapPost("/LogoutAllSessions", LogoutAllSessions)
            .RequireAuthorization()
            .WithName("LogoutAllSessions");

        group.MapPost("/KickOneSession/{sessionId}", KickOneSession)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("KickOneSession");

        group.MapPost("/KickManySessionsByUserId/{userId:guid}", KickManySessionsByUserId)
            .RequireAuthorization(policy => policy.RequireRole(UserRole.AdministratorString))
            .WithName("KickManySessionsByUserId");

        return group;
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> RequestRegistrationVerificationCode(
        [FromBody] RequestRegistrationVerificationCodeCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new RequestRegistrationVerificationCodeCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> Register(
        [FromBody] RegisterOneUserCommandArgs requestArgs,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) {
        var request = new RegisterOneUserCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        if (result.IsSuccess) {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
        }
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> RequestEmailRebindVerificationCode(
        [FromBody] RequestEmailRebindVerificationCodeCommandArgs requestArgs,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId)) {
            return TypedResults.Unauthorized();
        }

        var request = new RequestEmailRebindVerificationCodeCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> ConfirmEmailRebind(
        [FromBody] ConfirmEmailRebindCommandArgs requestArgs,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId)) {
            return TypedResults.Unauthorized();
        }

        var request = new ConfirmEmailRebindCommand(userId, requestArgs);
        var result = await sender.Send(request, cancellationToken);
        if (result.IsSuccess) {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
        }
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> LoginByEmailAddressAndPassword(
        [FromBody] LoginOneUserByEmailAddressAndPasswordCommandArgs requestArgs,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) {
        var request = new LoginOneUserByEmailAddressAndPasswordCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        if (result.IsSuccess) {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
        }
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> LoginByUniqueNameAndPassword(
        [FromBody] LoginOneUserByUniqueNameAndPasswordCommandArgs requestArgs,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) {
        var request = new LoginOneUserByUniqueNameAndPasswordCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        if (result.IsSuccess) {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
        }
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> RequestLoginVerificationCode(
        [FromBody] RequestLoginVerificationCodeCommandArgs requestArgs,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new RequestLoginVerificationCodeCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> LoginByEmailAddressAndVerificationCode(
        [FromBody] LoginOneUserByEmailAddressAndVerificationCodeCommandArgs requestArgs,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) {
        var request = new LoginOneUserByEmailAddressAndVerificationCodeCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        if (result.IsSuccess) {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
        }
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> LoginByUniqueNameAndVerificationCode(
        [FromBody] LoginOneUserByUniqueNameAndVerificationCodeCommandArgs requestArgs,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) {
        var request = new LoginOneUserByUniqueNameAndVerificationCodeCommand(requestArgs);
        var result = await sender.Send(request, cancellationToken);
        if (result.IsSuccess) {
            await AuthCookieHelper.SignInAsync(httpContext, result.Value);
        }
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> Logout(
        ISender sender,
        HttpContext httpContext
    ) {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId)) {
            return TypedResults.Unauthorized();
        }

        var request = new RemoveOneUserSessionCommand(new RemoveOneUserSessionCommandArgs {
            SessionId = httpContext.Connection.Id,
        });
        await sender.Send(request, CancellationToken.None);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return TypedResults.Ok(Result.Success());
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> LogoutAllSessions(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken
    ) {
        if (!AuthCookieHelper.TryGetCurrentUserId(httpContext, out var userId)) {
            return TypedResults.Unauthorized();
        }

        var request = new RemoveManyUserSessionsByUserIdCommand(new RemoveManyUserSessionsByUserIdCommandArgs {
            UserId = userId,
        });
        var result = await sender.Send(request, cancellationToken);
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> KickOneSession(
        string sessionId,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new RemoveOneUserSessionCommand(new RemoveOneUserSessionCommandArgs {
            SessionId = sessionId,
        });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static async Task<Microsoft.AspNetCore.Http.IResult> KickManySessionsByUserId(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken
    ) {
        var request = new RemoveManyUserSessionsByUserIdCommand(new RemoveManyUserSessionsByUserIdCommandArgs {
            UserId = userId,
        });
        var result = await sender.Send(request, cancellationToken);
        return ToHttpResult(result);
    }

    private static IResult ToHttpResult<T>(Sandlada.Extension.Auth.Domain.Commons.IResult<T> result) {
        if (result.IsSuccess) return TypedResults.Ok(result.Value);
        return ToFailureResult<T>(result.Error);
    }
    private static IResult ToFailureResult<T>(DomainError error) {
        if (error == DomainError.Auth.InvalidCredentials) return TypedResults.Unauthorized();
        if (error == DomainError.Auth.EmailAddressNotVerified) return TypedResults.Unauthorized();
        if (error == DomainError.User.NotFound) return TypedResults.NotFound(error);
        if (error == DomainError.Auth.SessionNotFound) return TypedResults.NotFound(error);
        if (error == DomainError.Auth.EmailRebindVerificationNotFound) return TypedResults.NotFound(error);
        if (error == DomainError.Auth.VerificationCodeNotFound) return TypedResults.NotFound(error);
        if (error == DomainError.Auth.PasswordLoginRequestLimitExceeded) return TypedResults.StatusCode(StatusCodes.Status429TooManyRequests);
        if (error == DomainError.Auth.PasswordLoginFailedAttemptLimitExceeded) return TypedResults.StatusCode(StatusCodes.Status429TooManyRequests);
        if (error == DomainError.Auth.AccountLocked) return TypedResults.StatusCode(StatusCodes.Status429TooManyRequests);
        return TypedResults.BadRequest(error);
    }
}
