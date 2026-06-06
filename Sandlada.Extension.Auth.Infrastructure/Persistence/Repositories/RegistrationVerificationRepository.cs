using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;

public sealed class RegistrationVerificationRepository : IRegistrationVerificationRepository {
    private readonly AuthDbContext dbContext;

    public RegistrationVerificationRepository(AuthDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public Task<IResult> InsertOneAsync(RegistrationVerification registrationVerification) {
        this.dbContext.RegistrationVerifications.Add(this.ToEntity(registrationVerification));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult<RegistrationVerification>> FindOneByEmailAddressAsync(EmailAddress emailAddress) {
        var normalizedEmailAddress = InfrastructureNormalization.Normalize(emailAddress.Value);
        var entity = await this.dbContext.RegistrationVerifications.AsNoTracking().FirstOrDefaultAsync(item => item.EmailAddressNormalized == normalizedEmailAddress);
        return entity is null
            ? Result.Failure<RegistrationVerification>(DomainError.Auth.VerificationCodeNotFound)
            : this.ToDomain(entity);
    }

    public Task<IResult> UpdateOneAsync(RegistrationVerification registrationVerification) {
        this.dbContext.RegistrationVerifications.Update(this.ToEntity(registrationVerification));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult> RemoveOneAsync(Guid id) {
        var entity = await this.dbContext.RegistrationVerifications.FirstOrDefaultAsync(item => item.Id == id);
        if (entity is null) return Result.Failure(DomainError.Auth.VerificationCodeNotFound);

        this.dbContext.RegistrationVerifications.Remove(entity);
        return Result.Success();
    }

    private IResult<RegistrationVerification> ToDomain(RegistrationVerificationEntity entity) {
        var emailAddressResult = EmailAddress.From(entity.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<RegistrationVerification>(emailAddressResult.Error);

        return RegistrationVerification.From(new RegistrationVerificationConstructorArgs {
            Id = entity.Id,
            EmailAddress = emailAddressResult.Value,
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

    private RegistrationVerificationEntity ToEntity(RegistrationVerification registrationVerification) {
        return new RegistrationVerificationEntity {
            Id = registrationVerification.Id,
            EmailAddress = registrationVerification.EmailAddress.Value,
            EmailAddressNormalized = InfrastructureNormalization.Normalize(registrationVerification.EmailAddress.Value),
            VerificationCodeHash = registrationVerification.VerificationCodeHash,
            ExpiresAt = registrationVerification.ExpiresAt,
            FailedAttemptCount = registrationVerification.FailedAttemptCount,
            RequestCount = registrationVerification.RequestCount,
            RequestCountDate = registrationVerification.RequestCountDate,
            CreatedAt = registrationVerification.CreatedAt,
            UpdatedAt = registrationVerification.UpdatedAt,
            ConsumedAt = registrationVerification.ConsumedAt,
        };
    }
}
