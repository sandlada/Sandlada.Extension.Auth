using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;

public sealed class LoginVerificationRepository : ILoginVerificationRepository {
    private readonly AuthDbContext dbContext;

    public LoginVerificationRepository(AuthDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public Task<IResult> InsertOneAsync(LoginVerification loginVerification) {
        this.dbContext.LoginVerifications.Add(this.ToEntity(loginVerification));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult<LoginVerification>> FindOneByEmailAddressAsync(EmailAddress emailAddress) {
        var normalizedEmailAddress = InfrastructureNormalization.Normalize(emailAddress.Value);
        var entity = await this.dbContext.LoginVerifications.AsNoTracking().FirstOrDefaultAsync(item => item.EmailAddressNormalized == normalizedEmailAddress);
        return entity is null
            ? Result.Failure<LoginVerification>(DomainError.Auth.VerificationCodeNotFound)
            : this.ToDomain(entity);
    }

    public Task<IResult> UpdateOneAsync(LoginVerification loginVerification) {
        this.dbContext.LoginVerifications.Update(this.ToEntity(loginVerification));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult> RemoveOneAsync(Guid id) {
        var entity = await this.dbContext.LoginVerifications.FirstOrDefaultAsync(item => item.Id == id);
        if (entity is null) return Result.Failure(DomainError.Auth.VerificationCodeNotFound);

        this.dbContext.LoginVerifications.Remove(entity);
        return Result.Success();
    }

    private IResult<LoginVerification> ToDomain(LoginVerificationEntity entity) {
        var emailAddressResult = EmailAddress.From(entity.EmailAddress);
        if (emailAddressResult.IsFailure) return Result.Failure<LoginVerification>(emailAddressResult.Error);

        return LoginVerification.From(new LoginVerificationConstructorArgs {
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

    private LoginVerificationEntity ToEntity(LoginVerification loginVerification) {
        return new LoginVerificationEntity {
            Id = loginVerification.Id,
            EmailAddress = loginVerification.EmailAddress.Value,
            EmailAddressNormalized = InfrastructureNormalization.Normalize(loginVerification.EmailAddress.Value),
            VerificationCodeHash = loginVerification.VerificationCodeHash,
            ExpiresAt = loginVerification.ExpiresAt,
            FailedAttemptCount = loginVerification.FailedAttemptCount,
            RequestCount = loginVerification.RequestCount,
            RequestCountDate = loginVerification.RequestCountDate,
            CreatedAt = loginVerification.CreatedAt,
            UpdatedAt = loginVerification.UpdatedAt,
            ConsumedAt = loginVerification.ConsumedAt,
        };
    }
}