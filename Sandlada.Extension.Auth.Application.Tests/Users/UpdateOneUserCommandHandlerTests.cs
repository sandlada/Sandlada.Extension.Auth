using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Application.Users;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.Users;

public sealed class UpdateOneUserCommandHandlerTests {
    [Fact]
    public async Task Handle_ValidUpdate_UpdatesTargetUser() {
        var user = CreateUser();
        var repository = new FakeUserRepository(user);
        var unitOfWork = new FakeUnitOfWork();
        var secretHashService = new FakeSecretHashService();
        var handler = new UpdateOneUserCommandHandler(repository, secretHashService, unitOfWork);

        var result = await handler.Handle(new UpdateOneUserCommand(user.Id, new UpdateOneUserCommandArgs {
            Role = UserRole.AdministratorString,
            Password = "updated-password",
            IsEmailVerified = false,
        }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.AdministratorString, result.Value.Role);
        Assert.False(result.Value.IsEmailVerified);
        Assert.Equal("hashed::updated-password", user.PasswordHash);
        Assert.Equal(1, unitOfWork.SaveChangesCallCount);
        Assert.True(repository.WasUpdated);
    }

    private static User CreateUser() {
        var utcNow = new DateTime(2026, 5, 31, 12, 0, 0, DateTimeKind.Utc);
        var userResult = User.From(new UserConstructorArgs {
            Id = Guid.NewGuid(),
            EmailAddress = EmailAddress.From("user@example.com").Value,
            UniqueName = "user",
            Role = UserRole.Normal,
            PasswordHash = "password-hash",
            IsEmailVerified = true,
            FirstLoginAt = null,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        });

        Assert.True(userResult.IsSuccess);
        return userResult.Value;
    }

    private sealed class FakeUserRepository(User user) : IUserRepository {
        public bool WasUpdated { get; private set; }

        public Task<IResult> InsertOneAsync(User user) => throw new NotSupportedException();

        public Task<IResult<User>> FindOneByIdAsync(Guid id) {
            return Task.FromResult(id == user.Id
                ? Result.Success(user)
                : Result.Failure<User>(DomainError.User.NotFound));
        }

        public Task<IResult<User>> FindOneByEmailAddressAsync(EmailAddress emailAddress) {
            return Task.FromResult(emailAddress.Value == user.EmailAddress.Value
                ? Result.Success(user)
                : Result.Failure<User>(DomainError.User.NotFound));
        }

        public Task<IResult<User>> FindOneByUniqueNameAsync(string uniqueName) {
            return Task.FromResult(uniqueName == user.UniqueName
                ? Result.Success(user)
                : Result.Failure<User>(DomainError.User.NotFound));
        }

        public Task<IResult> UpdateOneAsync(User updatedUser) {
            this.WasUpdated = true;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult> RemoveOneAsync(Guid id) => throw new NotSupportedException();
    }

    private sealed class FakeUnitOfWork : IApplicationUnitOfWork {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken) {
            this.SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeSecretHashService : ISecretHashService {
        public string Hash(string value) => $"hashed::{value}";

        public bool Verify(string value, string hash) => hash == this.Hash(value);
    }
}
