using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.Repositories;

public interface IEmailRebindVerificationRepository {
    Task<IResult> InsertOneAsync(EmailRebindVerification verification);

    Task<IResult<EmailRebindVerification>> FindOneByUserIdAsync(Guid userId);

    Task<IResult> UpdateOneAsync(EmailRebindVerification verification);

    Task<IResult> RemoveOneAsync(Guid id);
}
