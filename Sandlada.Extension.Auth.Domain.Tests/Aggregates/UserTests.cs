using Sandlada.Extension.Auth.Domain.Aggregates;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Tests.Aggregates;

public sealed class UserTests {
    [Fact]
    public void From_UniqueNameWithWhitespace_TrimmedAndSuccess() {
        var result = User.From(BuildArgs(uniqueName: "  unique-name  "));

        Assert.True(result.IsSuccess);
        Assert.Equal("unique-name", result.Value.UniqueName);
    }

    [Fact]
    public void From_PendingUserWithNullProfile_ReturnsSuccess() {
        var result = User.From(BuildArgs(uniqueName: null, isEmailVerified: false));

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.UniqueName);
        Assert.Null(result.Value.FirstLoginAt);
        Assert.False(result.Value.IsEmailVerified);
    }

    [Fact]
    public void From_FirstLoginAtProvided_AssignsValue() {
        var firstLoginAt = DateTime.UtcNow.AddMinutes(-5);

        var result = User.From(BuildArgs(firstLoginAt: firstLoginAt));

        Assert.True(result.IsSuccess);
        Assert.Equal(firstLoginAt, result.Value.FirstLoginAt);
    }

    [Fact]
    public void From_WhitespaceUnique_NormalizedToNull() {
        var result = User.From(BuildArgs(uniqueName: "   "));

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.UniqueName);
    }

    [Fact]
    public void From_UniqueNameTooShort_ReturnsFailure() {
        var result = User.From(BuildArgs(uniqueName: "ab"));

        AssertFailure(result, DomainError.User.UniqueNameTooShort.Code);
    }

    [Fact]
    public void From_UniqueNameAtBoundaryLengths_ReturnsSuccess() {
        var minResult = User.From(BuildArgs(uniqueName: new string('a', 3)));
        var maxResult = User.From(BuildArgs(uniqueName: new string('b', 50)));

        Assert.True(minResult.IsSuccess);
        Assert.True(maxResult.IsSuccess);
    }

    [Fact]
    public void From_UniqueNameTooLong_ReturnsFailure() {
        var result = User.From(BuildArgs(uniqueName: new string('c', 51)));

        AssertFailure(result, DomainError.User.UniqueNameTooLong.Code);
    }

    [Fact]
    public void From_DefaultStatusIsEnabled() {
        var result = User.From(BuildArgs(uniqueName: "unique-name"));

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Enabled, result.Value.Status);
    }

    [Fact]
    public void From_CustomStatus_AssignsValue() {
        var result = User.From(BuildArgs(uniqueName: "unique-name", status: UserStatus.Suspended));

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Suspended, result.Value.Status);
    }

    [Fact]
    public void From_EmptyPasswordHash_ReturnsFailure() {
        var result = User.From(BuildArgs(passwordHash: "   "));

        AssertFailure(result, DomainError.Auth.PasswordCannotBeEmpty.Code);
    }

    [Fact]
    public void UpdateStatus_ChangesValueAndUpdatedAt() {
        var user = CreateUser();
        var previousUpdatedAt = user.UpdatedAt;

        var result = user.UpdateStatus(UserStatus.Suspended);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserStatus.Suspended, user.Status);
        AssertUpdatedAt(user, previousUpdatedAt);
    }

    [Fact]
    public void UpdateUniqueName_ValidInput_TrimmedAndUpdatedAtChanged() {
        var user = CreateUser(uniqueName: "original-name");
        var previousUpdatedAt = user.UpdatedAt;

        var result = user.UpdateUniqueName("  next-unique-name  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("next-unique-name", user.UniqueName);
        AssertUpdatedAt(user, previousUpdatedAt);
    }

    [Fact]
    public void UpdateUniqueName_TooShort_ReturnsFailureWithoutMutation() {
        var user = CreateUser(uniqueName: "stable-name");
        var previousUniqueName = user.UniqueName;
        var previousUpdatedAt = user.UpdatedAt;

        var result = user.UpdateUniqueName("ab");

        AssertFailure(result, DomainError.User.UniqueNameTooShort.Code);
        Assert.Equal(previousUniqueName, user.UniqueName);
        Assert.Equal(previousUpdatedAt, user.UpdatedAt);
    }

    [Fact]
    public void UpdateUniqueName_TooLong_ReturnsFailureWithoutMutation() {
        var user = CreateUser(uniqueName: "stable-name");
        var previousUniqueName = user.UniqueName;
        var previousUpdatedAt = user.UpdatedAt;

        var result = user.UpdateUniqueName(new string('u', 51));

        AssertFailure(result, DomainError.User.UniqueNameTooLong.Code);
        Assert.Equal(previousUniqueName, user.UniqueName);
        Assert.Equal(previousUpdatedAt, user.UpdatedAt);
    }

    [Fact]
    public void UpdatePasswordHash_EmptyInput_ReturnsFailureWithoutMutation() {
        var user = CreateUser(passwordHash: "current-hash");
        var previousUpdatedAt = user.UpdatedAt;

        var result = user.UpdatePasswordHash("  ");

        AssertFailure(result, DomainError.Auth.PasswordCannotBeEmpty.Code);
        Assert.Equal("current-hash", user.PasswordHash);
        Assert.Equal(previousUpdatedAt, user.UpdatedAt);
    }

    [Fact]
    public void UpdatePasswordHash_ValidInput_UpdatesOnlyPasswordAndUpdatedAt() {
        var user = CreateUser(uniqueName: null, passwordHash: "old-hash", isEmailVerified: false);
        var previousUpdatedAt = user.UpdatedAt;

        var result = user.UpdatePasswordHash("new-hash");

        Assert.True(result.IsSuccess);
        Assert.Equal("new-hash", user.PasswordHash);
        Assert.Null(user.UniqueName);
        Assert.False(user.IsEmailVerified);
        AssertUpdatedAt(user, previousUpdatedAt);
    }

    [Fact]
    public void UpdateRole_ValidInput_ChangesRoleAndUpdatedAt() {
        var user = CreateUser(role: UserRole.Normal);
        var previousUpdatedAt = user.UpdatedAt;

        var result = user.UpdateRole(UserRole.Administrator);

        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.Administrator, user.Role);
        AssertUpdatedAt(user, previousUpdatedAt);
    }

    [Fact]
    public void UpdateEmailAddress_ChangesEmail_PreservesVerificationStatus() {
        var user = CreateUser(isEmailVerified: true);
        var previousUpdatedAt = user.UpdatedAt;

        var updateResult = user.UpdateEmailAddress(EmailAddress.From("new@example.com").Value);

        Assert.True(updateResult.IsSuccess);
        Assert.Equal("new@example.com", user.EmailAddress.Value);
        Assert.True(user.IsEmailVerified);
        AssertUpdatedAt(user, previousUpdatedAt);
    }

    [Fact]
    public void UpdateIsEmailVerified_SetsFlagAndUpdatedAt() {
        var user = CreateUser(isEmailVerified: false);
        var previousUpdatedAt = user.UpdatedAt;

        var updateResult = user.UpdateIsEmailVerified(true);

        Assert.True(updateResult.IsSuccess);
        Assert.True(user.IsEmailVerified);
        AssertUpdatedAt(user, previousUpdatedAt);
    }

    [Fact]
    public void MarkFirstLogin_FirstCallSetsTimestamp_SecondCallDoesNotMutate() {
        var user = CreateUser();
        var firstUpdatedAt = user.UpdatedAt;
        var firstLoginAt = DateTime.UtcNow.AddMinutes(-1);

        var firstResult = user.MarkFirstLogin(firstLoginAt);

        Assert.True(firstResult.IsSuccess);
        Assert.Equal(firstLoginAt, user.FirstLoginAt);
        AssertUpdatedAt(user, firstUpdatedAt);

        var updatedAfterFirstMark = user.UpdatedAt;
        var secondResult = user.MarkFirstLogin(DateTime.UtcNow);

        Assert.True(secondResult.IsSuccess);
        Assert.Equal(firstLoginAt, user.FirstLoginAt);
        Assert.Equal(updatedAfterFirstMark, user.UpdatedAt);
    }

    private static User CreateUser(
        string? uniqueName = "unique-name",
        string passwordHash = "hash-value",
        bool isEmailVerified = true,
        UserRole? role = null
    ) {
        var result = User.From(BuildArgs(uniqueName: uniqueName, passwordHash: passwordHash, isEmailVerified: isEmailVerified, role: role ?? UserRole.Normal));
        Assert.True(result.IsSuccess);
        return result.Value;
    }

    private static UserConstructorArgs BuildArgs(
        string? uniqueName = "unique-name",
        string passwordHash = "hash-value",
        bool isEmailVerified = true,
        UserRole? role = null,
        DateTime? firstLoginAt = null,
        UserStatus? status = null
    ) {
        var utcNow = DateTime.UtcNow;
        return new UserConstructorArgs {
            Id = Guid.NewGuid(),
            EmailAddress = EmailAddress.From("user@example.com").Value,
            UniqueName = uniqueName,
            Role = role ?? UserRole.Normal,
            PasswordHash = passwordHash,
            IsEmailVerified = isEmailVerified,
            Status = status ?? UserStatus.Enabled,
            FirstLoginAt = firstLoginAt,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
        };
    }

    private static void AssertFailure(IResult result, string expectedCode) {
        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
    }

    private static void AssertUpdatedAt(User user, DateTime previousUpdatedAt) {
        Assert.Equal(DateTimeKind.Utc, user.UpdatedAt.Kind);
        Assert.True(user.UpdatedAt >= previousUpdatedAt);
    }
}
