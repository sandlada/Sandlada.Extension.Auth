using Microsoft.AspNetCore.Builder;

namespace Sandlada.Extension.Auth.Api.Endpoints;

public static class UserProfileEndpointExtensions {
    /// <summary>
    /// Maps the UserProfile endpoints under "/Api/UserProfile".
    /// Call this after <c>UseAuthExtension()</c> if your application needs UserProfile functionality.
    /// </summary>
    public static WebApplication MapUserProfileEndpoints(this WebApplication app) {
        app.MapGroup("/Api/UserProfile").MapUserProfileEndpoints();
        return app;
    }
}