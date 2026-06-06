using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence;

public sealed class DevelopmentDatabaseInitializer(
    AuthDbContext dbContext,
    ISecretHashService secretHashService,
    IUserRepository userRepository,
    IApplicationUnitOfWork unitOfWork
) {
    public async Task InitializeAsync(CancellationToken cancellationToken) {
        await dbContext.Database.EnsureDeletedAsync(cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);

        foreach (var seedUser in GetSeedUsers()) {
            var emailAddressResult = EmailAddress.From(seedUser.EmailAddress);
            if (emailAddressResult.IsFailure) {
                throw new InvalidOperationException($"Invalid development seed email: {seedUser.EmailAddress}");
            }

            var utcNow = DateTime.UtcNow;
            var userResult = User.From(new UserConstructorArgs {
                Id = Guid.NewGuid(),
                EmailAddress = emailAddressResult.Value,
                UniqueName = seedUser.UniqueName,
                Role = seedUser.Role,
                PasswordHash = secretHashService.Hash(seedUser.Password),
                IsEmailVerified = true,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
            });

            if (userResult.IsFailure) {
                throw new InvalidOperationException($"Failed to create development seed user '{seedUser.EmailAddress}': {userResult.Error.Code}");
            }

            var insertResult = await userRepository.InsertOneAsync(userResult.Value);
            if (insertResult.IsFailure) {
                throw new InvalidOperationException($"Failed to persist development seed user '{seedUser.EmailAddress}': {insertResult.Error.Code}");
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<DevelopmentSeedUser> GetSeedUsers() {
        return [
            new DevelopmentSeedUser(
                EmailAddress: "user@example.com",
                Password: "user",
                UniqueName: "user",
                Role: UserRole.Normal
            ),
            new DevelopmentSeedUser(
                EmailAddress: "admin@example.com",
                Password: "admin",
                UniqueName: "admin",
                Role: UserRole.Administrator
            ),
        ];
    }

    private sealed record DevelopmentSeedUser(
        string EmailAddress,
        string Password,
        string UniqueName,
        UserRole Role
    );
}
