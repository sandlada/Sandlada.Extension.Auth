using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;

public sealed class AuthSessionRepository : IAuthSessionRepository {
    private readonly AuthDbContext dbContext;

    public AuthSessionRepository(AuthDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public async Task<IResult<int>> RemoveOneBySessionIdAsync(string sessionId) {
        var entity = await this.dbContext.AuthSessions.FirstOrDefaultAsync(item => item.SessionId == sessionId);
        if (entity is null) return Result.Success(0);

        this.dbContext.AuthSessions.Remove(entity);
        return Result.Success(1);
    }

    public async Task<IResult<int>> RemoveManyByUserIdAsync(Guid userId) {
        var entities = await this.dbContext.AuthSessions.Where(item => item.UserId == userId).ToListAsync();
        if (entities.Count == 0) return Result.Success(0);

        this.dbContext.AuthSessions.RemoveRange(entities);
        return Result.Success(entities.Count);
    }
}
