using System.ComponentModel;
using System.Text.Json;
using Sandlada.Extension.Auth.Domain.ValueObjects;

public sealed class GenderTests {

    [Theory]
    [InlineData("male", Gender.MaleString)]
    [InlineData("female", Gender.FemaleString)]
    [InlineData("unknown", Gender.UnknownString)]
    public void From_BuiltInValue_ReturnsCanonicalSingleton(string input, string expectedValue) {
        var result = Gender.From(input);

        Assert.True(result.IsSuccess);

        var expected = expectedValue switch {
            Gender.MaleString => Gender.Male,
            Gender.FemaleString => Gender.Female,
            Gender.UnknownString => Gender.Unknown,
            _ => throw new ArgumentOutOfRangeException(nameof(expectedValue), expectedValue, "Unknown expected gender.")
        };

        Assert.Same(expected, result.Value);
    }

    [Theory]
    [InlineData("male", Gender.MaleString)]
    [InlineData(" MALE ", Gender.MaleString)]
    [InlineData("female", Gender.FemaleString)]
    [InlineData(" Female ", Gender.FemaleString)]
    [InlineData("nonbinary", "nonbinary")]
    [InlineData("Agender", "Agender")]
    [InlineData("ab", "ab")]
    [InlineData("abcdefghijklmnop", "abcdefghijklmnop")]
    public void From_ValidInput_ReturnsExpectedValue(string input, string expected) {
        var result = Gender.From(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    [Theory]
    [InlineData("", "Gender.Empty", "Gender cannot be empty.")]
    [InlineData("   ", "Gender.Empty", "Gender cannot be empty.")]
    [InlineData("a", "Gender.ValueTooShort", "Gender value is too short. Minimum length is 2.")]
    [InlineData("abcdefghijklmnopq", "Gender.ValueTooLong", "Gender value is too long. Maximum length is 16.")]
    public void From_InvalidInput_ReturnsExpectedFailure(string input, string expectedCode, string expectedMessage) {
        var result = Gender.From(input);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
        Assert.Equal(expectedMessage, result.Error.Message);
    }

    [Theory]
    [InlineData("\"male\"", Gender.MaleString)]
    [InlineData("\" Female \"", Gender.FemaleString)]
    [InlineData("\"nonbinary\"", "nonbinary")]
    public void JsonConverter_Deserialize_StringPayload_ReturnsExpected(string json, string expected) {
        var result = JsonSerializer.Deserialize<Gender?>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result!.Value);
    }

    [Theory]
    [InlineData("{\"Value\":\"male\"}", Gender.MaleString)]
    [InlineData("{\"value\":\" Female \"}", Gender.FemaleString)]
    [InlineData("{\"Value\":\"nonbinary\"}", "nonbinary")]
    public void JsonConverter_Deserialize_ObjectPayload_ReturnsExpected(string json, string expected) {
        var result = JsonSerializer.Deserialize<Gender?>(json);

        Assert.NotNull(result);
        Assert.Equal(expected, result!.Value);
    }

    [Fact]
    public void JsonConverter_Deserialize_NullPayload_ReturnsNull() {
        var result = JsonSerializer.Deserialize<Gender?>("null");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"\"", "Gender cannot be null or empty string.")]
    [InlineData("\"   \"", "Gender cannot be null or empty string.")]
    [InlineData("\"a\"", "Gender value is too short. Minimum length is 2.")]
    [InlineData("{\"Other\":\"male\"}", "Gender cannot be empty.")]
    [InlineData("123", "Unexpected JSON payload for Gender.")]
    public void JsonConverter_Deserialize_InvalidInput_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Gender?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("male", "{\"Value\":\"male\"}")]
    [InlineData("Female", "{\"Value\":\"female\"}")]
    [InlineData("nonbinary", "{\"Value\":\"nonbinary\"}")]
    public void JsonConverter_Serialize_WritesExpectedObjectJson(string input, string expectedJson) {
        var gender = Gender.From(input).Value;

        var json = JsonSerializer.Serialize(gender);

        Assert.Equal(expectedJson, json);
    }

    [Theory]
    [InlineData("{\"Value\":\"male\"}", Gender.MaleString)]
    [InlineData(" {\"value\":\" Female \"} ", Gender.FemaleString)]
    [InlineData(" {\"value\":\"nonbinary\"} ", "nonbinary")]
    public void Parse_ObjectText_ReturnsExpectedGender(string input, string expected) {
        var result = Gender.Parse(input);

        Assert.Equal(expected, result.Value);
    }

    [Fact]
    public void TypeConverter_CanConvertFromAndToString_ReturnsTrue() {
        var converter = TypeDescriptor.GetConverter(typeof(Gender));

        Assert.True(converter.CanConvertFrom(typeof(string)));
        Assert.True(converter.CanConvertTo(typeof(string)));
    }

    [Theory]
    [InlineData("male", Gender.MaleString)]
    [InlineData("female", Gender.FemaleString)]
    [InlineData("nonbinary", "nonbinary")]
    public void TypeConverter_ConvertFromString_ValidInput_ReturnsExpectedGender(string input, string expected) {
        var converter = TypeDescriptor.GetConverter(typeof(Gender));

        var result = converter.ConvertFromInvariantString(input);

        var gender = Assert.IsType<Gender>(result);
        Assert.Equal(expected, gender.Value);
    }

    [Theory]
    [InlineData(Gender.MaleString)]
    [InlineData(Gender.FemaleString)]
    [InlineData("nonbinary")]
    public void TypeConverter_ConvertToString_ReturnsExpectedValue(string expected) {
        var converter = TypeDescriptor.GetConverter(typeof(Gender));
        var gender = Gender.From(expected).Value;

        var result = converter.ConvertToInvariantString(gender);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void TypeConverter_ConvertFromString_InvalidInput_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(Gender));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("a"));
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse() {
        var success = Gender.TryParse(null, null, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void Operators_ImplicitConversion_WorksAsExpected() {
        Gender? gender = "male";
        string? value = gender;

        Assert.NotNull(gender);
        Assert.Equal(Gender.MaleString, value);
    }

    [Fact]
    public void JsonConverter_RoundTrip_ReturnsEquivalentGender() {
        var source = Gender.From("nonbinary").Value;

        var json = JsonSerializer.Serialize(source);
        var roundTrip = JsonSerializer.Deserialize<Gender>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(source, roundTrip);
    }
}
