using System.Security.Claims;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using static OpenIddict.Abstractions.OpenIddictConstants;

using IResult = Microsoft.AspNetCore.Http.IResult;

namespace Sandlada.Extension.Auth.Api.Endpoints;

public static class OpenIddictEndpoints {
    public static IEndpointRouteBuilder MapOpenIddictEndpoints(this IEndpointRouteBuilder app) {
        app.MapGet("/Connect/Authorize", Authorize)
            .AllowAnonymous();

        app.MapPost("/Connect/Token", Exchange)
            .AllowAnonymous();

        app.MapGet("/Connect/UserInfo", Userinfo)
            .RequireAuthorization();

        app.MapGet("/Connect/Logout", Logout)
            .AllowAnonymous();

        return app;
    }

    private static async Task<IResult> Authorize(HttpContext httpContext) {
        var request = httpContext.GetOpenIddictServerRequest();
        if (request is null) {
            return Results.BadRequest(new {
                error = Errors.InvalidRequest,
                error_description = "The OpenID Connect request cannot be retrieved.",
            });
        }

        if (!httpContext.User.Identity?.IsAuthenticated ?? true) {
            return Results.Challenge(new AuthenticationProperties {
                RedirectUri = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.Path}{httpContext.Request.QueryString}",
            }, [CookieAuthenticationDefaults.AuthenticationScheme]);
        }

        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        identity.SetClaim(Claims.Subject, httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
            .SetClaim(Claims.Email, httpContext.User.FindFirst(ClaimTypes.Email)?.Value)
            .SetClaim(Claims.Name, httpContext.User.FindFirst(ClaimTypes.Name)?.Value)
            .SetClaim(Claims.Role, httpContext.User.FindFirst(ClaimTypes.Role)?.Value)
            .SetClaim("is_email_verified", httpContext.User.FindFirst("is_email_verified")?.Value);

        var uniqueName = httpContext.User.FindFirst("unique_name")?.Value;
        if (uniqueName is not null) {
            identity.SetClaim("unique_name", uniqueName);
        }

        identity.SetScopes(request.GetScopes());
        identity.SetResources("resource_server");
        identity.SetDestinations(GetDestinations);

        return Results.SignIn(
            new ClaimsPrincipal(identity),
            properties: new AuthenticationProperties(),
            authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    private static async Task<IResult> Exchange(HttpContext httpContext) {
        var request = httpContext.GetOpenIddictServerRequest();
        if (request is null) {
            return Results.BadRequest(new {
                error = Errors.InvalidRequest,
                error_description = "The OpenID Connect request cannot be retrieved.",
            });
        }

        if (request.IsAuthorizationCodeGrantType() || request.IsRefreshTokenGrantType()) {
            var result = await httpContext.AuthenticateAsync(OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

            var identity = new ClaimsIdentity(
                result.Principal?.Claims ?? [],
                authenticationType: TokenValidationParameters.DefaultAuthenticationType,
                nameType: Claims.Name,
                roleType: Claims.Role);

            identity.SetDestinations(GetDestinations);

            return Results.SignIn(
                new ClaimsPrincipal(identity),
                properties: new AuthenticationProperties(),
                authenticationScheme: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        return Results.BadRequest(new {
            error = Errors.UnsupportedGrantType,
            error_description = "The specified grant type is not supported.",
        });
    }

    private static async Task<IResult> Userinfo(HttpContext httpContext) {
        var user = httpContext.User;
        if (user.Identity?.IsAuthenticated != true) {
            return Results.Challenge(
                authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme],
                properties: new AuthenticationProperties(new Dictionary<string, string?> {
                    [OpenIddictServerAspNetCoreConstants.Properties.Error] = Errors.InvalidToken,
                    [OpenIddictServerAspNetCoreConstants.Properties.ErrorDescription] =
                        "The specified access token is invalid.",
                }));
        }

        var claims = new Dictionary<string, object>(StringComparer.Ordinal) {
            [Claims.Subject] = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            [Claims.Email] = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
            [Claims.Name] = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
            [Claims.Role] = user.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty,
        };

        var uniqueName = user.FindFirst("unique_name")?.Value;
        if (uniqueName is not null) {
            claims["unique_name"] = uniqueName;
        }

        var isEmailVerified = user.FindFirst("is_email_verified")?.Value;
        if (isEmailVerified is not null) {
            claims["is_email_verified"] = bool.Parse(isEmailVerified);
        }

        return Results.Ok(claims);
    }

    private static async Task<IResult> Logout(HttpContext httpContext) {
        var request = httpContext.GetOpenIddictServerRequest();
        if (request is null) {
            return Results.BadRequest(new {
                error = Errors.InvalidRequest,
                error_description = "The OpenID Connect request cannot be retrieved.",
            });
        }

        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return Results.SignOut(
            properties: new AuthenticationProperties {
                RedirectUri = request.PostLogoutRedirectUri ?? "/",
            },
            authenticationSchemes: [OpenIddictServerAspNetCoreDefaults.AuthenticationScheme]);
    }

    private static IEnumerable<string> GetDestinations(Claim claim) {
        switch (claim.Type) {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;
                if (claim.Subject?.HasScope(Scopes.Profile) == true) {
                    yield return Destinations.IdentityToken;
                }
                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;
                if (claim.Subject?.HasScope(Scopes.Email) == true) {
                    yield return Destinations.IdentityToken;
                }
                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;
                if (claim.Subject?.HasScope(Scopes.Roles) == true) {
                    yield return Destinations.IdentityToken;
                }
                yield break;

            case Claims.Subject:
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            case "is_email_verified":
            case "unique_name":
                yield return Destinations.AccessToken;
                yield return Destinations.IdentityToken;
                yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}