using System.ComponentModel;
using System.Text.Json;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

public sealed class UserStatusTests {
    [Theory]
    [InlineData("Disabled", nameof(UserStatus.Disabled))]
    [InlineData(" disabled ", nameof(UserStatus.Disabled))]
    [InlineData("Enabled", nameof(UserStatus.Enabled))]
    [InlineData("ENABLED", nameof(UserStatus.Enabled))]
    [InlineData("Suspended", nameof(UserStatus.Suspended))]
    [InlineData("Blocked", nameof(UserStatus.Blocked))]
    [InlineData("blocked", nameof(UserStatus.Blocked))]
    [InlineData(" Deleted ", nameof(UserStatus.Deleted))]
    public void From_ValidInput_ReturnsSuccess(string input, string expectedName) {
        var result = UserStatus.From(new UserStatusConstructorArgs { Code = input });
        var expected = ResolveStatus(expectedName);

        Assert.True(result.IsSuccess);
        Assert.Same(expected, result.Value);
    }

    [Theory]
    [InlineData("", "The user status is invalid.")]
    [InlineData(" ", "The user status is invalid.")]
    [InlineData("Unknown", "The user status is invalid.")]
    [InlineData("Inactive", "The user status is invalid.")]
    [InlineData("Active", "The user status is invalid.")]
    public void From_InvalidInput_ReturnsExpectedFailure(string input, string expectedMessage) {
        var result = UserStatus.From(new UserStatusConstructorArgs { Code = input });

        Assert.True(result.IsFailure);
        Assert.Equal(DomainError.UserStatus.InvalidStatus.Code, result.Error.Code);
        Assert.Equal(expectedMessage, result.Error.Message);
    }

    [Fact]
    public void From_NullArgs_ThrowsArgumentNullException() {
        Assert.Throws<ArgumentNullException>(() => UserStatus.From(null!));
    }

    [Theory]
    [InlineData("\"Enabled\"", nameof(UserStatus.Enabled))]
    [InlineData("\"enabled\"", nameof(UserStatus.Enabled))]
    [InlineData("\" suspended \"", nameof(UserStatus.Suspended))]
    public void JsonConverter_Deserialize_String_ReturnsExpectedStatus(string json, string expectedName) {
        var result = JsonSerializer.Deserialize<UserStatus?>(json);

        Assert.NotNull(result);
        Assert.Same(ResolveStatus(expectedName), result);
    }

    [Theory]
    [InlineData("{\"Code\":\"Blocked\"}", nameof(UserStatus.Blocked))]
    [InlineData("{\"code\":\" deleted \"}", nameof(UserStatus.Deleted))]
    public void JsonConverter_Deserialize_Object_ReturnsExpectedStatus(string json, string expectedName) {
        var result = JsonSerializer.Deserialize<UserStatus?>(json);

        Assert.NotNull(result);
        Assert.Same(ResolveStatus(expectedName), result);
    }

    [Fact]
    public void JsonConverter_Deserialize_Null_ReturnsNull() {
        var result = JsonSerializer.Deserialize<UserStatus?>("null");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"\"", "UserStatus cannot be null or empty string.")]
    [InlineData("\"   \"", "UserStatus cannot be null or empty string.")]
    [InlineData("\"Inactive\"", "The user status is invalid.")]
    [InlineData("\"Active\"", "The user status is invalid.")]
    [InlineData("{\"Other\":\"Active\"}", "The user status is invalid.")]
    [InlineData("123", "Unexpected JSON payload for UserStatus.")]
    public void JsonConverter_Deserialize_InvalidInput_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UserStatus?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("Enabled", "{\"Code\":\"Enabled\"}")]
    [InlineData(" suspended ", "{\"Code\":\"Suspended\"}")]
    public void JsonConverter_Serialize_WritesExpectedJsonObject(string input, string expectedJson) {
        var status = UserStatus.From(new UserStatusConstructorArgs { Code = input }).Value;

        var json = JsonSerializer.Serialize(status);

        Assert.Equal(expectedJson, json);
    }

    [Theory]
    [InlineData("{\"Code\":\"Blocked\"}", nameof(UserStatus.Blocked))]
    [InlineData(" {\"code\":\" deleted \"} ", nameof(UserStatus.Deleted))]
    public void Parse_ObjectText_ReturnsExpectedStatus(string input, string expectedName) {
        var result = UserStatus.Parse(input);

        Assert.Same(ResolveStatus(expectedName), result);
    }

    [Fact]
    public void Parse_NullInput_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => UserStatus.Parse(null!));
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse() {
        var result = UserStatus.TryParse(null, null, out var status);

        Assert.False(result);
        Assert.Null(status);
    }

    [Fact]
    public void JsonConverter_RoundTrip_ReturnsEquivalentStatus() {
        var status = UserStatus.Blocked;

        var json = JsonSerializer.Serialize(status);
        var roundTrip = JsonSerializer.Deserialize<UserStatus>(json);

        Assert.Same(status, roundTrip);
    }

    [Fact]
    public void TypeConverter_CanConvertFromAndToString_ReturnsTrue() {
        var converter = TypeDescriptor.GetConverter(typeof(UserStatus));

        Assert.True(converter.CanConvertFrom(typeof(string)));
        Assert.True(converter.CanConvertTo(typeof(string)));
    }

    [Theory]
    [InlineData("disabled", nameof(UserStatus.Disabled))]
    [InlineData("ENABLED", nameof(UserStatus.Enabled))]
    [InlineData("Suspended", nameof(UserStatus.Suspended))]
    [InlineData(" Blocked ", nameof(UserStatus.Blocked))]
    public void TypeConverter_ConvertFromString_ValidInput_ReturnsExpectedStatus(string input, string expectedName) {
        var converter = TypeDescriptor.GetConverter(typeof(UserStatus));

        var result = converter.ConvertFromInvariantString(input);

        var status = Assert.IsType<UserStatus>(result);
        Assert.Same(ResolveStatus(expectedName), status);
    }

    [Theory]
    [InlineData(nameof(UserStatus.Disabled))]
    [InlineData(nameof(UserStatus.Enabled))]
    [InlineData(nameof(UserStatus.Suspended))]
    [InlineData(nameof(UserStatus.Blocked))]
    [InlineData(nameof(UserStatus.Deleted))]
    public void TypeConverter_ConvertToString_ReturnsExpectedValue(string expectedName) {
        var converter = TypeDescriptor.GetConverter(typeof(UserStatus));
        var status = ResolveStatus(expectedName);

        var result = converter.ConvertToInvariantString(status);

        Assert.Equal(status.Code, result);
    }

    [Fact]
    public void TypeConverter_ConvertFromString_InvalidInput_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(UserStatus));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("Inactive"));
    }

    [Fact]
    public void TypeConverter_ConvertFromString_WhitespaceOnly_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(UserStatus));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("   "));
    }

    [Fact]
    public void Operators_ImplicitConversion_WorkAsExpected() {
        UserStatus? status = "enabled";
        string? value = status;

        Assert.Same(UserStatus.Enabled, status);
        Assert.Equal("Enabled", value);
    }

    [Fact]
    public void Operators_ImplicitConversion_InvalidString_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => {
            UserStatus? _ = "active";
        });
    }

    private static UserStatus ResolveStatus(string expectedName) {
        return expectedName switch {
            nameof(UserStatus.Disabled) => UserStatus.Disabled,
            nameof(UserStatus.Enabled) => UserStatus.Enabled,
            nameof(UserStatus.Suspended) => UserStatus.Suspended,
            nameof(UserStatus.Blocked) => UserStatus.Blocked,
            nameof(UserStatus.Deleted) => UserStatus.Deleted,
            _ => throw new ArgumentOutOfRangeException(nameof(expectedName), expectedName, "Unknown status name.")
        };
    }
}
