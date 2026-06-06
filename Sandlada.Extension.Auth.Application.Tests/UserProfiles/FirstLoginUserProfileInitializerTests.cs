using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.UserProfiles;

public sealed class FirstLoginUserProfileInitializerTests {
    [Fact]
    public async Task InitializeAsync_FirstLoginPendingAndSettingMissing_CreatesSettingAndMarksUser() {
        var repository = new FakeUserProfileRepository();
        var initializer = new FirstLoginUserProfileInitializer(repository);
        var user = CreateUser(firstLoginAt: null);

        var result = await initializer.InitializeAsync(user, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
        Assert.NotNull(user.FirstLoginAt);
        Assert.NotNull(repository.InsertedUserProfile);
        Assert.Equal(user.Id, repository.InsertedUserProfile!.UserId);
        Assert.Equal(1, repository.FindCalls);
    }

    [Fact]
    public async Task InitializeAsync_FirstLoginAlreadyCompleted_DoesNothing() {
        var repository = new FakeUserProfileRepository();
        var initializer = new FirstLoginUserProfileInitializer(repository);
        var firstLoginAt = new DateTime(2026, 5, 31, 8, 0, 0, DateTimeKind.Utc);
        var user = CreateUser(firstLoginAt);

        var result = await initializer.InitializeAsync(user, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
        Assert.Equal(firstLoginAt, user.FirstLoginAt);
        Assert.Null(repository.InsertedUserProfile);
        Assert.Equal(0, repository.FindCalls);
    }

    private static User CreateUser(DateTime? firstLoginAt) {
        var utcNow = new DateTime(2026, 5, 31, 9, 0, 0, DateTimeKind.Utc);
        var userResult = User.From(new UserConstructorArgs {
            Id = Guid.NewGuid(),
            EmailAddress = EmailAddress.From("user@example.com").Value,
            UniqueName = "user",
            Role = UserRole.Normal,
            PasswordHash = "hashed-password",
            IsEmailVerified = true,
            FirstLoginAt = firstLoginAt,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        });

        Assert.True(userResult.IsSuccess);
        return userResult.Value;
    }

    private sealed class FakeUserProfileRepository : IUserProfileRepository {
        public int FindCalls { get; private set; }
        public UserProfile? InsertedUserProfile { get; private set; }

        public Task<IResult> InsertOneAsync(UserProfile UserProfile) {
            this.InsertedUserProfile = UserProfile;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult<UserProfile>> FindOneByUserIdAsync(Guid userId) {
            this.FindCalls++;
            return Task.FromResult<IResult<UserProfile>>(Result.Failure<UserProfile>(DomainError.UserProfile.NotFound));
        }

        public Task<IResult> UpdateOneAsync(UserProfile UserProfile) {
            throw new NotSupportedException();
        }

        public Task<IResult> RemoveOneByUserIdAsync(Guid userId) {
            throw new NotSupportedException();
        }
    }
}
