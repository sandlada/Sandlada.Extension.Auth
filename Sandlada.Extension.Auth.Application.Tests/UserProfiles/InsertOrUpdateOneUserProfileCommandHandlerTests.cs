using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.UserProfiles;

public sealed class InsertOrUpdateOneUserProfileCommandHandlerTests
{
    private static readonly DateTime UtcNow = new(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_NoExistingProfile_CreatesNewProfile()
    {
        var repo = new FakeUserProfileRepository(profileExists: false);
        var handler = new InsertOrUpdateOneUserProfileCommandHandler(repo, new FakeUnitOfWork());

        var result = await handler.Handle(new InsertOrUpdateOneUserProfileCommand(
            UserId,
            new InsertOrUpdateOneUserProfileCommandArgs
            {
                DisplayName = "New User",
                Gender = "male",
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(repo.WasInserted);
        Assert.Equal(UserId, result.Value.UserId);
    }

    [Fact]
    public async Task Handle_ExistingProfile_UpdatesProfile()
    {
        var existingProfile = CreateDefaultProfile();
        var repo = new FakeUserProfileRepository(existingProfile);
        var handler = new InsertOrUpdateOneUserProfileCommandHandler(repo, new FakeUnitOfWork());

        var result = await handler.Handle(new InsertOrUpdateOneUserProfileCommand(
            UserId,
            new InsertOrUpdateOneUserProfileCommandArgs
            {
                DisplayName = "Updated Name",
                IsDarkMode = true,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(repo.WasUpdated);
        Assert.Equal(UserId, result.Value.UserId);
    }

    [Fact]
    public async Task Handle_ExistingProfile_AppliesUpdates()
    {
        var existingProfile = CreateDefaultProfile();
        var repo = new FakeUserProfileRepository(existingProfile);
        var handler = new InsertOrUpdateOneUserProfileCommandHandler(repo, new FakeUnitOfWork());

        var result = await handler.Handle(new InsertOrUpdateOneUserProfileCommand(
            UserId,
            new InsertOrUpdateOneUserProfileCommandArgs
            {
                DisplayName = "Updated Name",
                IsDarkMode = true,
            }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Updated Name", result.Value.displayName);
        Assert.True(result.Value.IsDarkMode);
    }

    private static UserProfile CreateDefaultProfile()
    {
        var contrastResult = MaterialContrastLevel.From(new MaterialContrastLevelConstructorArgs { Level = 0 });
        Assert.True(contrastResult.IsSuccess);

        var variantResult = MaterialVariant.From(0);
        Assert.True(variantResult.IsSuccess);

        var result = UserProfile.From(new UserProfileConstructorArgs
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            SourceColorArgb = 0xFF0078D4,
            IsDarkMode = false,
            ContrastLevel = contrastResult.Value,
            Variant = variantResult.Value,
            DisplayName = "Default",
            Gender = Gender.Unknown,
            PreferredLanguage = null,
            Metadata = null,
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private sealed class FakeUserProfileRepository : IUserProfileRepository
    {
        private UserProfile? _profile;
        private readonly bool _profileExists;
        public bool WasInserted { get; private set; }
        public bool WasUpdated { get; private set; }

        public FakeUserProfileRepository(bool profileExists)
        {
            _profileExists = profileExists;
        }

        public FakeUserProfileRepository(UserProfile profile)
        {
            _profile = profile;
            _profileExists = true;
        }

        public Task<IResult> InsertOneAsync(UserProfile userProfile)
        {
            WasInserted = true;
            _profile = userProfile;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult<UserProfile>> FindOneByUserIdAsync(Guid userId) =>
            Task.FromResult(_profileExists && _profile is not null
                ? Result.Success(_profile)
                : Result.Failure<UserProfile>(DomainError.UserProfile.NotFound));

        public Task<IResult> UpdateOneAsync(UserProfile userProfile)
        {
            WasUpdated = true;
            _profile = userProfile;
            return Task.FromResult<IResult>(Result.Success());
        }

        public Task<IResult> RemoveOneByUserIdAsync(Guid userId) => throw new NotSupportedException();
    }

    internal sealed class FakeUnitOfWork : IApplicationUnitOfWork
    {
        public int SaveChangesCallCount { get; set; }
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }
    }
}
