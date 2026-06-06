using Sandlada.Extension.Auth.Api.Endpoints;
using Sandlada.Extension.Auth.Application;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure;
using Sandlada.Extension.Auth.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
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

builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme)
    .Configure<Microsoft.AspNetCore.Authentication.Cookies.ITicketStore>((options, ticketStore) => {
        options.SessionStore = ticketStore;
    });

builder.Services.AddAuthorization(options => {
    options.AddPolicy(UserRole.AdministratorString, policy => policy.RequireRole(UserRole.AdministratorString));
    options.AddPolicy(UserRole.NormalString, policy => policy.RequireRole(UserRole.NormalString));
});

builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngular4200", policy => {
        policy.WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("AllowAngular4200");

using (var scope = app.Services.CreateScope()) {
    if (app.Environment.IsDevelopment()) {
        var initializer = scope.ServiceProvider.GetRequiredService<DevelopmentDatabaseInitializer>();
        await initializer.InitializeAsync(app.Lifetime.ApplicationStopping);
    } else {
        var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
        await dbContext.Database.MigrateAsync(app.Lifetime.ApplicationStopping);
    }
}

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
app.MapGroup("/Api/UserProfile").MapUserProfileEndpoints();

app.Run();
