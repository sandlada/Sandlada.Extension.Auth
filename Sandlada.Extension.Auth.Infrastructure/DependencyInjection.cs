using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Infrastructure.Security;
using Sandlada.Extension.Auth.Infrastructure.Persistence;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Sandlada.Extension.Auth.Infrastructure;

public static class DependencyInjection {
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration) {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.Configure<SmtpVerificationCodeSenderOptions>(configuration.GetSection(SmtpVerificationCodeSenderOptions.SectionName));

        services.AddDbContextFactory<AuthDbContext>(options => options.UseSqlite(connectionString));
        services.AddScoped<AuthDbContext>(serviceProvider => serviceProvider.GetRequiredService<IDbContextFactory<AuthDbContext>>().CreateDbContext());
        services.AddScoped<DevelopmentDatabaseInitializer>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserProfileRepository, UserProfileRepository>();
        services.AddScoped<IRegistrationVerificationRepository, RegistrationVerificationRepository>();
        services.AddScoped<IEmailRebindVerificationRepository, EmailRebindVerificationRepository>();
        services.AddScoped<ILoginVerificationRepository, LoginVerificationRepository>();
        services.AddScoped<IAuthSessionRepository, AuthSessionRepository>();
        services.AddScoped<IApplicationUnitOfWork>(serviceProvider => serviceProvider.GetRequiredService<AuthDbContext>());

        services.AddSingleton<ISecretHashService, SecretHashService>();
        services.AddSingleton<IRegistrationVerificationCodeGenerator, RegistrationVerificationCodeGenerator>();
        services.AddSingleton<NoopRegistrationVerificationCodeSender>();
        services.AddSingleton<SmtpRegistrationVerificationCodeSender>();
        services.AddSingleton<IRegistrationVerificationCodeSender>(serviceProvider => {
            var smtpOptions = serviceProvider.GetRequiredService<IOptions<SmtpVerificationCodeSenderOptions>>().Value;
            if (!smtpOptions.Enabled) {
                return serviceProvider.GetRequiredService<NoopRegistrationVerificationCodeSender>();
            }

            if (SmtpRegistrationVerificationCodeSender.IsConfigured(smtpOptions)) {
                return serviceProvider.GetRequiredService<SmtpRegistrationVerificationCodeSender>();
            }

            var env = serviceProvider.GetRequiredService<Microsoft.Extensions.Hosting.IHostEnvironment>();
            if (env.IsDevelopment()) {
                return serviceProvider.GetRequiredService<NoopRegistrationVerificationCodeSender>();
            }

            throw new InvalidOperationException("Email SMTP is enabled but incomplete. Configure Email:Smtp:Host, Port, and FromAddress.");
        });
        services.AddSingleton<Microsoft.AspNetCore.Authentication.Cookies.ITicketStore, AuthSessionTicketStore>();

        return services;
    }
}
