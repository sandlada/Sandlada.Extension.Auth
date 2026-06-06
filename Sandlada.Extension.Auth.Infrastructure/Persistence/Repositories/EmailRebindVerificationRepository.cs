using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;

public sealed class EmailRebindVerificationRepository : IEmailRebindVerificationRepository {
    private readonly AuthDbContext dbContext;

    public EmailRebindVerificationRepository(AuthDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public Task<IResult> InsertOneAsync(EmailRebindVerification verification) {
        this.dbContext.EmailRebindVerifications.Add(this.ToEntity(verification));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult<EmailRebindVerification>> FindOneByUserIdAsync(Guid userId) {
        var entity = await this.dbContext.EmailRebindVerifications.AsNoTracking().FirstOrDefaultAsync(item => item.UserId == userId);
        return entity is null
            ? Result.Failure<EmailRebindVerification>(DomainError.Auth.EmailRebindVerificationNotFound)
            : this.ToDomain(entity);
    }

    public Task<IResult> UpdateOneAsync(EmailRebindVerification verification) {
        this.dbContext.EmailRebindVerifications.Update(this.ToEntity(verification));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult> RemoveOneAsync(Guid id) {
        var entity = await this.dbContext.EmailRebindVerifications.FirstOrDefaultAsync(item => item.Id == id);
        if (entity is null) return Result.Failure(DomainError.Auth.EmailRebindVerificationNotFound);

        this.dbContext.EmailRebindVerifications.Remove(entity);
        return Result.Success();
    }

    private IResult<EmailRebindVerification> ToDomain(EmailRebindVerificationEntity entity) {
        var targetEmailAddressResult = EmailAddress.From(entity.TargetEmailAddress);
        if (targetEmailAddressResult.IsFailure) return Result.Failure<EmailRebindVerification>(targetEmailAddressResult.Error);

        return EmailRebindVerification.From(new EmailRebindVerificationConstructorArgs {
            Id = entity.Id,
            UserId = entity.UserId,
            TargetEmailAddress = targetEmailAddressResult.Value,
            VerificationCodeHash = entity.VerificationCodeHash,
            ExpiresAt = entity.ExpiresAt,
            FailedAttemptCount = entity.FailedAttemptCount,
            RequestCount = entity.RequestCount,
            RequestCountDate = entity.RequestCountDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ConsumedAt = entity.ConsumedAt,
        });
    }

    private EmailRebindVerificationEntity ToEntity(EmailRebindVerification verification) {
        return new EmailRebindVerificationEntity {
            Id = verification.Id,
            UserId = verification.UserId,
            TargetEmailAddress = verification.TargetEmailAddress.Value,
            TargetEmailAddressNormalized = InfrastructureNormalization.Normalize(verification.TargetEmailAddress.Value),
            VerificationCodeHash = verification.VerificationCodeHash,
            ExpiresAt = verification.ExpiresAt,
            FailedAttemptCount = verification.FailedAttemptCount,
            RequestCount = verification.RequestCount,
            RequestCountDate = verification.RequestCountDate,
            CreatedAt = verification.CreatedAt,
            UpdatedAt = verification.UpdatedAt,
            ConsumedAt = verification.ConsumedAt,
        };
    }
}
