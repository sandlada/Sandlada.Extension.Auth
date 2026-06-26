using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext : DbContext, IApplicationUnitOfWork {
    public DbSet<UserEntity> Users => this.Set<UserEntity>();
    public DbSet<UserProfileEntity> UserProfiles => this.Set<UserProfileEntity>();
    public DbSet<RegistrationVerificationEntity> RegistrationVerifications => this.Set<RegistrationVerificationEntity>();
    public DbSet<EmailRebindVerificationEntity> EmailRebindVerifications => this.Set<EmailRebindVerificationEntity>();
    public DbSet<LoginVerificationEntity> LoginVerifications => this.Set<LoginVerificationEntity>();
    public DbSet<AuthSessionEntity> AuthSessions => this.Set<AuthSessionEntity>();
    public DbSet<OAuthClientEntity> OAuthClients => this.Set<OAuthClientEntity>();
    public DbSet<PasswordLoginAttemptEntity> PasswordLoginAttempts => this.Set<PasswordLoginAttemptEntity>();

    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options) {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
        modelBuilder.UseOpenIddict();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) {
        return base.SaveChangesAsync(cancellationToken);
    }
}
