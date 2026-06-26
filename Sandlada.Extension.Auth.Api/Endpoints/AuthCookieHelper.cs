using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sandlada.Extension.Auth.Application.Auth;

namespace Sandlada.Extension.Auth.Api.Endpoints;

/// <summary>
/// Shared helpers for cookie-based sign-in and current user id extraction.
/// Used by REST endpoints and GraphQL resolvers.
/// </summary>
internal static class AuthCookieHelper
{
    public static async Task SignInAsync(HttpContext httpContext, AuthenticatedUserResponse user)
    {
        var uniqueName = string.IsNullOrWhiteSpace(user.UniqueName) ? null : user.UniqueName;
        var principalName = uniqueName ?? user.EmailAddress;

        var claims = new List<Claim> {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Email, user.EmailAddress),
            new(ClaimTypes.Name, principalName),
            new(ClaimTypes.Role, user.Role),
            new("is_email_verified", user.IsEmailVerified.ToString()),
        };

        if (uniqueName is not null)
        {
            claims.Add(new Claim("unique_name", uniqueName));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var properties = new AuthenticationProperties
        {
            AllowRefresh = true,
            IsPersistent = true,
            IssuedUtc = DateTimeOffset.UtcNow,
            ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7),
        };

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, properties);
    }

    public static bool TryGetCurrentUserId(HttpContext httpContext, out Guid userId)
    {
        var rawUserId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(rawUserId, out userId);
    }
}
