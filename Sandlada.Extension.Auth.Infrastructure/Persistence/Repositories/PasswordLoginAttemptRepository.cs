using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;

public sealed class PasswordLoginAttemptRepository : IPasswordLoginAttemptRepository {
    private readonly AuthDbContext dbContext;

    public PasswordLoginAttemptRepository(AuthDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public Task<IResult> InsertOneAsync(PasswordLoginAttempt attempt) {
        this.dbContext.PasswordLoginAttempts.Add(this.ToEntity(attempt));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult<PasswordLoginAttempt>> FindOneByEmailAddressAsync(EmailAddress emailAddress) {
        var normalizedEmailAddress = InfrastructureNormalization.Normalize(emailAddress.Value);
        var entity = await this.dbContext.PasswordLoginAttempts.AsNoTracking().FirstOrDefaultAsync(item => item.EmailAddressNormalized == normalizedEmailAddress);
        return entity is null
            ? Result.Failure<PasswordLoginAttempt>(DomainError.Auth.VerificationCodeNotFound)
            : this.ToDomain(entity);
    }

    public Task<IResult> UpdateOneAsync(PasswordLoginAttempt attempt) {
        this.dbContext.PasswordLoginAttempts.Update(this.ToEntity(attempt));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult> RemoveOneAsync(Guid id) {
        var entity = await this.dbContext.PasswordLoginAttempts.FirstOrDefaultAsync(item => item.Id == id);
        if (entity is null) return Result.Failure(DomainError.Auth.VerificationCodeNotFound);

        this.dbContext.PasswordLoginAttempts.Remove(entity);
        return Result.Success();
    }

    private IResult<PasswordLoginAttempt> ToDomain(PasswordLoginAttemptEntity entity) {
        var emailAddressResult = EmailAddress.From(entity.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<PasswordLoginAttempt>(emailAddressResult.Error);

        return PasswordLoginAttempt.From(new PasswordLoginAttemptConstructorArgs {
            Id = entity.Id,
            EmailAddress = emailAddressResult.Value,
            FailedAttemptCount = entity.FailedAttemptCount,
            LockoutEnd = entity.LockoutEnd,
            RequestCount = entity.RequestCount,
            RequestCountDate = entity.RequestCountDate,
            LastFailedAttemptAt = entity.LastFailedAttemptAt,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        });
    }

    private PasswordLoginAttemptEntity ToEntity(PasswordLoginAttempt attempt) {
        return new PasswordLoginAttemptEntity {
            Id = attempt.Id,
            EmailAddress = attempt.EmailAddress.Value,
            EmailAddressNormalized = InfrastructureNormalization.Normalize(attempt.EmailAddress.Value),
            FailedAttemptCount = attempt.FailedAttemptCount,
            LockoutEnd = attempt.LockoutEnd,
            RequestCount = attempt.RequestCount,
            RequestCountDate = attempt.RequestCountDate,
            LastFailedAttemptAt = attempt.LastFailedAttemptAt,
            CreatedAt = attempt.CreatedAt,
            UpdatedAt = attempt.UpdatedAt,
        };
    }
}
