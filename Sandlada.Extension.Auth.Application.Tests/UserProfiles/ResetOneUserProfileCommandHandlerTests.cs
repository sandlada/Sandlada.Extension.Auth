using Sandlada.Extension.Auth.Application.Auth;
using Sandlada.Extension.Auth.Application.UserProfiles;
using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Repositories;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Application.Tests.UserProfiles;

public sealed class ResetOneUserProfileCommandHandlerTests
{
    private static readonly DateTime UtcNow = new(2026, 6, 26, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid UserId = Guid.NewGuid();

    [Fact]
    public async Task Handle_ExistingProfile_ResetsToDefaults()
    {
        var profile = CreateCustomizedProfile();
        var repo = new FakeUserProfileRepository(profile);
        var handler = new ResetOneUserProfileCommandHandler(repo, new FakeUnitOfWork());

        var result = await handler.Handle(new ResetOneUserProfileCommand(new ResetOneUserProfileCommandArgs { UserId = UserId }), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(repo.WasUpdated);
        // After reset, should have default values
        Assert.False(result.Value.IsDarkMode);
        Assert.Equal(0, result.Value.ContrastLevel);
        Assert.Equal((uint)0xFF0078D4, result.Value.SourceColorArgb);
    }

    [Fact]
    public async Task Handle_NonexistentProfile_ReturnsNotFound()
    {
        var repo = new FakeUserProfileRepository(profileExists: false);
        var handler = new ResetOneUserProfileCommandHandler(repo, new FakeUnitOfWork());

        var result = await handler.Handle(new ResetOneUserProfileCommand(new ResetOneUserProfileCommandArgs { UserId = UserId }), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.UserProfile.NotFound, result.Error);
    }

    private static UserProfile CreateCustomizedProfile()
    {
        var contrastResult = MaterialContrastLevel.From(new MaterialContrastLevelConstructorArgs { Level = 1 });
        Assert.True(contrastResult.IsSuccess);

        var variantResult = MaterialVariant.From(2);
        Assert.True(variantResult.IsSuccess);

        var result = UserProfile.From(new UserProfileConstructorArgs
        {
            Id = Guid.NewGuid(),
            UserId = UserId,
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            SourceColorArgb = 0xFFFF0000,
            IsDarkMode = true,
            ContrastLevel = contrastResult.Value,
            Variant = variantResult.Value,
            DisplayName = "Custom",
            Gender = Gender.Female,
            PreferredLanguage = "zh-TW",
            Metadata = "{\"key\":\"value\"}",
        });
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private sealed class FakeUserProfileRepository : IUserProfileRepository
    {
        private UserProfile? _profile;
        private readonly bool _profileExists;
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

        public Task<IResult> InsertOneAsync(UserProfile userProfile) => throw new NotSupportedException();

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
