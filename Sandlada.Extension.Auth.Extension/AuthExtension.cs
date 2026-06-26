using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sandlada.Extension.Auth.Api.Endpoints;
using Sandlada.Extension.Auth.Api.GraphQL;
using Sandlada.Extension.Auth.Application;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure;
using Sandlada.Extension.Auth.Infrastructure.Persistence;

namespace Sandlada.Extension.Auth;

public static class AuthExtension {
    /// <summary>
    /// Registers all Sandlada Extension Auth services including:
    /// MediatR, EF Core, cookie authentication, authorization policies,
    /// and Swagger/OpenAPI.
    /// </summary>
    public static IServiceCollection AddAuthExtension(
        this IServiceCollection services,
        IConfiguration configuration
    ) {
        // 1. Application layer (MediatR)
        services.AddApplication();

        // 2. Infrastructure layer (EF Core, repositories, external services)
        services.AddInfrastructure(configuration);

        // 3. Endpoints & OpenAPI
        services.AddEndpointsApiExplorer();
        services.AddOpenApi();

        // 4. Cookie Authentication
        services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options => {
                options.Cookie.Name = "__Host-Sandlada.Auth";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
                options.Events = new CookieAuthenticationEvents {
                    OnRedirectToLogin = context => {
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        return Task.CompletedTask;
                    },
                    OnRedirectToAccessDenied = context => {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        return Task.CompletedTask;
                    },
                };
            });

        // 6. Server-side session ticket store
        services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
            .Configure<ITicketStore>((options, ticketStore) => {
                options.SessionStore = ticketStore;
            });

        // 7. Authorization policies
        services.AddAuthorization(options => {
            options.AddPolicy(UserRole.AdministratorString, policy =>
                policy.RequireRole(UserRole.AdministratorString));
            options.AddPolicy(UserRole.NormalString, policy =>
                policy.RequireRole(UserRole.NormalString));
        });

        return services;
    }

    /// <summary>
    /// Configures the middleware pipeline: database initialization,
    /// Swagger UI (dev only), HTTPS redirection, authentication,
    /// authorization, and controller mapping.
    /// </summary>
    public static WebApplication UseAuthExtension(this WebApplication app) {
        // Database initialization
        using (var scope = app.Services.CreateScope()) {
            if (app.Environment.IsDevelopment()) {
                var initializer = scope.ServiceProvider.GetRequiredService<DevelopmentDatabaseInitializer>();
                initializer.InitializeAsync(app.Lifetime.ApplicationStopping).GetAwaiter().GetResult();
            } else {
                var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
                dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping).GetAwaiter().GetResult();
            }
        }

        // Swagger UI (development only)
        if (app.Environment.IsDevelopment()) {
            app.MapOpenApi();
            app.UseSwaggerUI(options => {
                options.SwaggerEndpoint("/openapi/v1.json", "Sandlada Extension Auth API v1");
            });
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapGroup("/Api/Auth").MapAuthEndpoints();
        app.MapGroup("/Api/User").MapUserEndpoints();

        return app;
    }

    /// <summary>
    /// (Optional) Registers the GraphQL server (HotChocolate) for Sandlada Auth.
    /// Call this in addition to AddAuthExtension if you want /graphql support in a consuming host.
    /// </summary>
    public static IServiceCollection AddGraphQLAuthExtension(this IServiceCollection services)
    {
        services
            .AddGraphQLServer()
            .AddQueryType<Query>()
            .AddMutationType<Mutation>();

        return services;
    }

    /// <summary>
    /// (Optional) Maps the GraphQL endpoint (/graphql). Call after UseAuthExtension (or equivalent auth middleware).
    /// </summary>
    public static WebApplication UseGraphQLAuthExtension(this WebApplication app)
    {
        app.MapGraphQL();
        return app;
    }
}
