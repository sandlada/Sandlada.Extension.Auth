using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Repositories;

public sealed class OAuthClientRepository : IOAuthClientRepository {
    private readonly AuthDbContext dbContext;

    public OAuthClientRepository(AuthDbContext dbContext) {
        this.dbContext = dbContext;
    }

    public Task<IResult> InsertOneAsync(OAuthClient client) {
        this.dbContext.OAuthClients.Add(this.ToEntity(client));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult<OAuthClient>> FindOneByIdAsync(Guid id) {
        var entity = await this.dbContext.OAuthClients.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id);
        return entity is null
            ? Result.Failure<OAuthClient>(DomainError.OAuthClient.NotFound)
            : this.ToDomain(entity);
    }

    public async Task<IResult<OAuthClient>> FindOneByClientIdAsync(string clientId) {
        var entity = await this.dbContext.OAuthClients.AsNoTracking().FirstOrDefaultAsync(item => item.ClientId == clientId);
        return entity is null
            ? Result.Failure<OAuthClient>(DomainError.OAuthClient.NotFound)
            : this.ToDomain(entity);
    }

    public async Task<IResult<List<OAuthClient>>> FindManyAsync() {
        var entities = await this.dbContext.OAuthClients.AsNoTracking().ToListAsync();
        var clients = new List<OAuthClient>();
        foreach (var entity in entities) {
            var domainResult = this.ToDomain(entity);
            if (domainResult.IsFailure) {
                return Result.Failure<List<OAuthClient>>(domainResult.Error);
            }
            clients.Add(domainResult.Value);
        }
        return Result.Success(clients);
    }

    public Task<IResult> UpdateOneAsync(OAuthClient client) {
        this.dbContext.OAuthClients.Update(this.ToEntity(client));
        return Task.FromResult<IResult>(Result.Success());
    }

    public async Task<IResult> RemoveOneAsync(Guid id) {
        var entity = await this.dbContext.OAuthClients.FirstOrDefaultAsync(item => item.Id == id);
        if (entity is null) return Result.Failure(DomainError.OAuthClient.NotFound);

        this.dbContext.OAuthClients.Remove(entity);
        return Result.Success();
    }

    private IResult<OAuthClient> ToDomain(OAuthClientEntity entity) {
        return OAuthClient.From(new OAuthClientConstructorArgs {
            Id = entity.Id,
            ClientId = entity.ClientId,
            DisplayName = entity.DisplayName,
            RedirectUris = DeserializeList(entity.RedirectUris),
            PostLogoutRedirectUris = DeserializeList(entity.PostLogoutRedirectUris),
            AllowedScopes = DeserializeList(entity.AllowedScopes),
            AllowedGrantTypes = DeserializeList(entity.AllowedGrantTypes),
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
        });
    }

    private OAuthClientEntity ToEntity(OAuthClient client) {
        return new OAuthClientEntity {
            Id = client.Id,
            ClientId = client.ClientId,
            DisplayName = client.DisplayName,
            RedirectUris = SerializeList(client.RedirectUris),
            PostLogoutRedirectUris = SerializeList(client.PostLogoutRedirectUris),
            AllowedScopes = SerializeList(client.AllowedScopes),
            AllowedGrantTypes = SerializeList(client.AllowedGrantTypes),
            CreatedAt = client.CreatedAt,
            UpdatedAt = client.UpdatedAt,
        };
    }

    private static string SerializeList(List<string> items) {
        return items.Count == 0 ? string.Empty : string.Join(',', items);
    }

    private static List<string> DeserializeList(string value) {
        return string.IsNullOrWhiteSpace(value) ? [] : [..value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
    }
}