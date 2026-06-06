
using System.ComponentModel;
using System.Text.Json;
using Sandlada.Extension.Auth.Domain.ValueObjects;

public sealed class UserRoleTests {

    [Fact]
    public void From_NullInput_ReturnsExpectedFailure() {
        var result = UserRole.From(null!);

        Assert.True(result.IsFailure);
        Assert.Equal("UserRole.InvalidValue", result.Error.Code);
    }

    [Theory]
    [InlineData("Normal", UserRole.NormalString)]
    [InlineData("normal", UserRole.NormalString)]
    [InlineData("Administrator", UserRole.AdministratorString)]
    [InlineData("administrator", UserRole.AdministratorString)]
    [InlineData(" NORMAL ", UserRole.NormalString)]
    [InlineData(" Administrator ", UserRole.AdministratorString)]
    public void From_ValidInput_ReturnsSuccess(string input, string expected) {
        var result = UserRole.From(input);
        var expectedResult = UserRole.From(expected);
        Assert.True(result.IsSuccess);
        Assert.True(expectedResult.IsSuccess);
        Assert.Equal(expectedResult.Value, result.Value);
    }

    [Theory]
    [InlineData("", "Invalid UserRole: ''. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData(" ", "Invalid UserRole: ' '. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("User", "Invalid UserRole: 'User'. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("guest", "Invalid UserRole: 'guest'. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("admin", "Invalid UserRole: 'admin'. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("norm", "Invalid UserRole: 'norm'. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("administrater", "Invalid UserRole: 'administrater'. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("normalnormal", "Invalid UserRole: 'normalnormal'. Valid values are 'Administrator' or 'Normal'.")]
    public void From_InvalidInput_ReturnsExpectedFailure(string input, string expectedMessage) {
        var result = UserRole.From(input);

        Assert.True(result.IsFailure);
        Assert.Equal("UserRole.InvalidValue", result.Error.Code);
        Assert.Equal(expectedMessage, result.Error.Message);
    }

    [Theory]
    [InlineData("\"normal\"", UserRole.NormalString)]
    [InlineData("\"administrator\"", UserRole.AdministratorString)]
    [InlineData("\" NORMAL \"", UserRole.NormalString)]
    public void JsonConverter_Deserialize_ValidInput_ReturnsExpectedRole(string json, string expected) {
        var result = JsonSerializer.Deserialize<UserRole?>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("{\"Value\":\"normal\"}", UserRole.NormalString)]
    [InlineData("{\"value\":\" administrator \"}", UserRole.AdministratorString)]
    public void JsonConverter_Deserialize_Object_ReturnsExpectedRole(string json, string expected) {
        var result = JsonSerializer.Deserialize<UserRole?>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result!.Value);
    }

    [Fact]
    public void JsonConverter_Deserialize_Null_ReturnsNull() {
        var result = JsonSerializer.Deserialize<UserRole?>("null");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"\"", "UserRole cannot be null or empty string.")]
    [InlineData("\"   \"", "UserRole cannot be null or empty string.")]
    [InlineData("\"User\"", "Invalid UserRole: 'User'. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("{\"Other\":\"normal\"}", "Invalid UserRole: ''. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("{\"value\":\"guest\"}", "Invalid UserRole: 'guest'. Valid values are 'Administrator' or 'Normal'.")]
    [InlineData("123", "Unexpected JSON payload for UserRole.")]
    public void JsonConverter_Deserialize_InvalidInput_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<UserRole?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(" NORMAL ", "{\"Value\":\"normal\"}")]
    [InlineData("Administrator", "{\"Value\":\"administrator\"}")]
    public void JsonConverter_Serialize_WritesExpectedJson(string input, string expectedJson) {
        var role = UserRole.From(input).Value;

        var json = JsonSerializer.Serialize(role);

        Assert.Equal(expectedJson, json);
    }

    [Theory]
    [InlineData("{\"Value\":\"normal\"}", UserRole.NormalString)]
    [InlineData(" {\"value\":\" administrator \"} ", UserRole.AdministratorString)]
    public void Parse_ObjectText_ReturnsExpectedRole(string input, string expected) {
        var result = UserRole.Parse(input);

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void TypeConverter_CanConvertFromAndToString_ReturnsTrue() {
        var converter = TypeDescriptor.GetConverter(typeof(UserRole));

        Assert.True(converter.CanConvertFrom(typeof(string)));
        Assert.True(converter.CanConvertTo(typeof(string)));
    }

    [Theory]
    [InlineData("normal", UserRole.NormalString)]
    [InlineData("administrator", UserRole.AdministratorString)]
    [InlineData(" NORMAL ", UserRole.NormalString)]
    public void TypeConverter_ConvertFromString_ValidInput_ReturnsExpectedRole(string input, string expected) {
        var converter = TypeDescriptor.GetConverter(typeof(UserRole));

        var result = converter.ConvertFromInvariantString(input);

        var role = Assert.IsType<UserRole>(result);
        Assert.Equal(expected, role.Value);
    }

    [Theory]
    [InlineData(UserRole.NormalString)]
    [InlineData(UserRole.AdministratorString)]
    public void TypeConverter_ConvertToString_ReturnsExpectedValue(string expected) {
        var converter = TypeDescriptor.GetConverter(typeof(UserRole));
        var role = UserRole.From(expected).Value;

        var result = converter.ConvertToInvariantString(role);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TypeConverter_ConvertFromString_InvalidInput_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(UserRole));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("User"));
    }

    [Fact]
    public void TypeConverter_ConvertFromString_WhitespaceOnly_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(UserRole));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("   "));
    }

    [Fact]
    public void Parse_NullInput_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => UserRole.Parse(null!));
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse() {
        var result = UserRole.TryParse(null, null, out var role);

        Assert.False(result);
        Assert.Null(role);
    }

    [Fact]
    public void JsonConverter_RoundTrip_ReturnsEquivalentRole() {
        var role = UserRole.Administrator;

        var json = JsonSerializer.Serialize(role);
        var roundTrip = JsonSerializer.Deserialize<UserRole>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(role, roundTrip);
    }

    [Fact]
    public void Operators_ImplicitConversion_NullString_ReturnsNullRole() {
        UserRole? role = (string?) null;

        Assert.Null(role);
    }

    [Fact]
    public void Operators_ImplicitConversion_InvalidString_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => {
            UserRole? _ = "guest";
        });
    }

}
