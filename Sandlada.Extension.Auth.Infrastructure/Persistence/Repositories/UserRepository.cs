using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository {
    private readonly AuthDbContext dbContext;

    public UserRepository(AuthDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public Task<IResult> InsertOneAsync(User user) {
        this.dbContext.Users.Add(this.ToEntity(user));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult<User>> FindOneByIdAsync(Guid id) {
        var entity = await this.dbContext.Users.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return entity is null
            ? Result.Failure<User>(DomainError.User.NotFound)
            : this.ToDomain(entity);
    }

    public async Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress) {
        var normalizedEmailAddress = InfrastructureNormalization.Normalize(emailAddress.Value);
        var entity = await this.dbContext.Users.AsNoTracking().FirstOrDefaultAsync(item => item.EmailAddressNormalized == normalizedEmailAddress);
        return entity is null
            ? Result.Failure<User>(DomainError.User.NotFound)
            : this.ToDomain(entity);
    }

    public async Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) {
        if (string.IsNullOrWhiteSpace(uniqueName)) {
            return Result.Failure<User>(DomainError.User.NotFound);
        }

        var normalizedUniqueName = InfrastructureNormalization.Normalize(uniqueName);
        var entity = await this.dbContext.Users.AsNoTracking().FirstOrDefaultAsync(item => item.UniqueNameNormalized == normalizedUniqueName);
        return entity is null
            ? Result.Failure<User>(DomainError.User.NotFound)
            : this.ToDomain(entity);
    }

    public Task<IResult> UpdateOneAsync(User user) {
        this.dbContext.Users.Update(this.ToEntity(user));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult> RemoveOneAsync(Guid id) {
        var entity = await this.dbContext.Users.FirstOrDefaultAsync(item => item.Id == id);
        if (entity is null) return Result.Failure(DomainError.User.NotFound);

        this.dbContext.Users.Remove(entity);
        return Result.Success();
    }

    private IResult<User> ToDomain(UserEntity entity) {
        var emailAddressResult = EmailAddress.From(entity.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<User>(emailAddressResult.Error);

        var roleResult = UserRole.From(entity.Role);
        if (roleResult.IsFailure) return Result.Failure<User>(roleResult.Error);

        var statusResult = UserStatus.From(new UserStatusConstructorArgs { Code = entity.Status });
        if (statusResult.IsFailure) return Result.Failure<User>(statusResult.Error);

        return User.From(new UserConstructorArgs {
            Id = entity.Id,
            EmailAddress = emailAddressResult.Value,
            UniqueName = entity.UniqueName,
            Role = roleResult.Value,
            PasswordHash = entity.PasswordHash,
            IsEmailVerified = entity.IsEmailVerified,
            FirstLoginAt = entity.FirstLoginAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Status = statusResult.Value,
        });
    }

    private UserEntity ToEntity(User user) {
        return new UserEntity {
            Id = user.Id,
            EmailAddress = user.EmailAddress.Value,
            EmailAddressNormalized = InfrastructureNormalization.Normalize(user.EmailAddress.Value),
            UniqueName = user.UniqueName,
            UniqueNameNormalized = InfrastructureNormalization.NormalizeNullable(user.UniqueName),
            Status = user.Status.Code,
            Role = user.Role.Value,
            PasswordHash = user.PasswordHash,
            IsEmailVerified = user.IsEmailVerified,
            FirstLoginAt = user.FirstLoginAt,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
        };
    }
}
