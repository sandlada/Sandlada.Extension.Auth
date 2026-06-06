using System.Security.Claims;
using Sandlada.Extension.Auth.Infrastructure.Persistence;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Security;

public sealed class AuthSessionTicketStore : ITicketStore {
    private readonly IDbContextFactory<AuthDbContext> dbContextFactory;

    public AuthSessionTicketStore(IDbContextFactory<AuthDbContext> dbContextFactory) {
        this.dbContextFactory = dbContextFactory;
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket) {
        var sessionId = Guid.NewGuid().ToString("N");
        var utcNow = DateTime.UtcNow;
        var entity = new AuthSessionEntity {
            SessionId = sessionId,
            UserId = this.GetUserId(ticket),
            TicketData = TicketSerializer.Default.Serialize(ticket),
            ExpiresAt = ticket.Properties.ExpiresUtc,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };

        await using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
        dbContext.AuthSessions.Add(entity);
        await dbContext.SaveChangesAsync();
        return sessionId;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket) {
        await using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
        var entity = await dbContext.AuthSessions.FirstOrDefaultAsync(item => item.SessionId == key);
        if (entity is null) return;

        entity.UserId = this.GetUserId(ticket);
        entity.TicketData = TicketSerializer.Default.Serialize(ticket);
        entity.ExpiresAt = ticket.Properties.ExpiresUtc;
        entity.UpdatedAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync();
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key) {
        await using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
        var entity = await dbContext.AuthSessions.FirstOrDefaultAsync(item => item.SessionId == key);
        if (entity is null) return null;

        if (entity.ExpiresAt is not null && entity.ExpiresAt <= DateTimeOffset.UtcNow) {
            dbContext.AuthSessions.Remove(entity);
            await dbContext.SaveChangesAsync();
            return null;
        }

        return TicketSerializer.Default.Deserialize(entity.TicketData);
    }

    public async Task RemoveAsync(string key) {
        await using var dbContext = await this.dbContextFactory.CreateDbContextAsync();
        var entity = await dbContext.AuthSessions.FirstOrDefaultAsync(item => item.SessionId == key);
        if (entity is null) return;

        dbContext.AuthSessions.Remove(entity);
        await dbContext.SaveChangesAsync();
    }

    private Guid GetUserId(AuthenticationTicket ticket) {
        var userIdValue = ticket.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdValue, out var userId)) {
            throw new InvalidOperationException("The authentication ticket does not contain a valid user ID claim.");
        }

        return userId;
    }
}
