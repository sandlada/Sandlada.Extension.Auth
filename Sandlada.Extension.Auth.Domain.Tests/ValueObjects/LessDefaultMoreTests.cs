using System.ComponentModel;
using System.Text.Json;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Tests.ValueObjects;

public sealed class LessDefaultMoreTests {
    [Theory]
    [InlineData("Less", LessDefaultMore.LessString)]
    [InlineData(" less ", LessDefaultMore.LessString)]
    [InlineData("DEFAULT", LessDefaultMore.DefaultString)]
    [InlineData("More", LessDefaultMore.MoreString)]
    public void Parse_ValidText_ReturnsExpectedValue(string input, string expectedValue) {
        var result = LessDefaultMore.Parse(input);

        Assert.Equal(expectedValue, result.Value);
        Assert.Same(Resolve(expectedValue), result);
    }

    [Theory]
    [InlineData("{\"Value\":\"Less\"}", LessDefaultMore.LessString)]
    [InlineData("{\"value\":\" default \"}", LessDefaultMore.DefaultString)]
    [InlineData("{\"Value\":\"More\"}", LessDefaultMore.MoreString)]
    public void Parse_ObjectText_ReturnsExpectedValue(string input, string expectedValue) {
        var result = LessDefaultMore.Parse(input);

        Assert.Equal(expectedValue, result.Value);
    }

    [Fact]
    public void Parse_InvalidValue_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => LessDefaultMore.Parse("extreme"));
    }

    [Fact]
    public void Parse_NullInput_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => LessDefaultMore.Parse(null!));
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse() {
        var success = LessDefaultMore.TryParse(null, null, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"Less\"", LessDefaultMore.LessString)]
    [InlineData("\" default \"", LessDefaultMore.DefaultString)]
    [InlineData("\"MORE\"", LessDefaultMore.MoreString)]
    public void JsonConverter_Deserialize_StringPayload_ReturnsExpectedValue(string json, string expectedValue) {
        var result = JsonSerializer.Deserialize<LessDefaultMore?>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedValue, result!.Value);
    }

    [Theory]
    [InlineData("{\"Value\":\"Less\"}", LessDefaultMore.LessString)]
    [InlineData("{\"value\":\"Default\"}", LessDefaultMore.DefaultString)]
    [InlineData("{\"Value\":\"More\"}", LessDefaultMore.MoreString)]
    public void JsonConverter_Deserialize_ObjectPayload_ReturnsExpectedValue(string json, string expectedValue) {
        var result = JsonSerializer.Deserialize<LessDefaultMore?>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedValue, result!.Value);
    }

    [Fact]
    public void JsonConverter_Deserialize_NullPayload_ReturnsNull() {
        var result = JsonSerializer.Deserialize<LessDefaultMore?>("null");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"\"", "LessDefaultMore cannot be null or empty string.")]
    [InlineData("\"  \"", "LessDefaultMore cannot be null or empty string.")]
    [InlineData("\"Extreme\"", "Invalid LessDefaultMore value.")]
    [InlineData("{\"Other\":\"Less\"}", "Invalid LessDefaultMore value.")]
    [InlineData("123", "Unexpected JSON payload for LessDefaultMore.")]
    public void JsonConverter_Deserialize_InvalidPayload_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<LessDefaultMore?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData(LessDefaultMore.LessString, "{\"Value\":\"Less\"}")]
    [InlineData(LessDefaultMore.DefaultString, "{\"Value\":\"Default\"}")]
    [InlineData(LessDefaultMore.MoreString, "{\"Value\":\"More\"}")]
    public void JsonConverter_Serialize_WritesExpectedObjectJson(string input, string expectedJson) {
        var source = LessDefaultMore.Parse(input);

        var json = JsonSerializer.Serialize(source);

        Assert.Equal(expectedJson, json);
    }

    [Fact]
    public void JsonConverter_RoundTrip_ReturnsEquivalentObject() {
        var source = LessDefaultMore.More;

        var json = JsonSerializer.Serialize(source);
        var roundTrip = JsonSerializer.Deserialize<LessDefaultMore>(json);

        Assert.NotNull(roundTrip);
        Assert.Same(source, roundTrip);
    }

    [Fact]
    public void TypeConverter_CanConvertFromAndToString_ReturnsTrue() {
        var converter = TypeDescriptor.GetConverter(typeof(LessDefaultMore));

        Assert.True(converter.CanConvertFrom(typeof(string)));
        Assert.True(converter.CanConvertTo(typeof(string)));
    }

    [Theory]
    [InlineData("less", LessDefaultMore.LessString)]
    [InlineData(" Default ", LessDefaultMore.DefaultString)]
    [InlineData("MORE", LessDefaultMore.MoreString)]
    public void TypeConverter_ConvertFromString_ValidInput_ReturnsExpectedValue(string input, string expectedValue) {
        var converter = TypeDescriptor.GetConverter(typeof(LessDefaultMore));

        var result = converter.ConvertFromInvariantString(input);

        var value = Assert.IsType<LessDefaultMore>(result);
        Assert.Equal(expectedValue, value.Value);
    }

    [Fact]
    public void TypeConverter_ConvertToString_ReturnsExpectedValue() {
        var converter = TypeDescriptor.GetConverter(typeof(LessDefaultMore));

        var result = converter.ConvertToInvariantString(LessDefaultMore.Default);

        Assert.Equal(LessDefaultMore.DefaultString, result);
    }

    [Fact]
    public void TypeConverter_ConvertFromString_InvalidInput_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(LessDefaultMore));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("Extreme"));
    }

    [Fact]
    public void Operators_ImplicitConversion_WorkAsExpected() {
        LessDefaultMore? value = "more";
        string? text = value;

        Assert.Same(LessDefaultMore.More, value);
        Assert.Equal(LessDefaultMore.MoreString, text);
    }

    [Fact]
    public void Operators_ImplicitConversion_InvalidInput_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => {
            LessDefaultMore? _ = "extreme";
        });
    }

    private static LessDefaultMore Resolve(string value) {
        return value switch {
            LessDefaultMore.LessString => LessDefaultMore.Less,
            LessDefaultMore.DefaultString => LessDefaultMore.Default,
            LessDefaultMore.MoreString => LessDefaultMore.More,
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown preference value.")
        };
    }
}
