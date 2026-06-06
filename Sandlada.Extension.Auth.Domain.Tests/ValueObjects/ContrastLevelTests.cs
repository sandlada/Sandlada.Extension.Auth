using System.ComponentModel;
using System.Text.Json;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Tests.ValueObjects;

public sealed class ContrastLevelTests {
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void From_ValidInput_ReturnsCanonicalInstance(int input) {
        var result = MaterialContrastLevel.From(new MaterialContrastLevelConstructorArgs { Level = input });

        Assert.True(result.IsSuccess);

        var expected = input switch {
            -1 => MaterialContrastLevel.Reduced,
            0 => MaterialContrastLevel.Default,
            1 => MaterialContrastLevel.High,
            _ => throw new ArgumentOutOfRangeException(nameof(input), input, "Unexpected contrast level."),
        };

        Assert.Same(expected, result.Value);
    }

    [Fact]
    public void From_InvalidInput_ReturnsExpectedFailure() {
        var result = MaterialContrastLevel.From(new MaterialContrastLevelConstructorArgs { Level = 2 });

        Assert.True(result.IsFailure);
        Assert.Equal("MaterialContrastLevel.InvalidLevel", result.Error.Code);
        Assert.Equal("Invalid contrast level: '2'. Valid values are -1, 0, or 1.", result.Error.Message);
    }

    [Theory]
    [InlineData("-1", -1)]
    [InlineData("0", 0)]
    [InlineData("1", 1)]
    [InlineData("Reduced", -1)]
    [InlineData("default", 0)]
    [InlineData("HIGH", 1)]
    public void Parse_ValidString_ReturnsExpectedLevel(string input, int expectedLevel) {
        var result = MaterialContrastLevel.Parse(input);

        Assert.Equal(expectedLevel, result.Level);
    }

    [Theory]
    [InlineData("{\"Level\":-1}", -1)]
    [InlineData("{\"level\":\"0\"}", 0)]
    [InlineData("{\"Level\":\"High\"}", 1)]
    public void Parse_ObjectText_ReturnsExpectedLevel(string input, int expectedLevel) {
        var result = MaterialContrastLevel.Parse(input);

        Assert.Equal(expectedLevel, result.Level);
    }

    [Fact]
    public void Parse_ObjectText_InvalidPropertyValue_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => MaterialContrastLevel.Parse("{\"Level\":\"VeryHigh\"}"));
    }

    [Fact]
    public void Parse_InvalidInput_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => MaterialContrastLevel.Parse("2"));
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse() {
        var result = MaterialContrastLevel.TryParse(null, null, out var level);

        Assert.False(result);
        Assert.Null(level);
    }

    [Theory]
    [InlineData("-1", -1)]
    [InlineData("1", 1)]
    [InlineData("\"0\"", 0)]
    [InlineData("\"reduced\"", -1)]
    public void JsonConverter_Deserialize_ScalarInput_ReturnsExpectedLevel(string json, int expectedLevel) {
        var result = JsonSerializer.Deserialize<MaterialContrastLevel?>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedLevel, result!.Level);
    }

    [Theory]
    [InlineData("{\"Level\":-1}", -1)]
    [InlineData("{\"level\":\"0\"}", 0)]
    [InlineData("{\"Level\":\"High\"}", 1)]
    public void JsonConverter_Deserialize_ObjectInput_ReturnsExpectedLevel(string json, int expectedLevel) {
        var result = JsonSerializer.Deserialize<MaterialContrastLevel?>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedLevel, result!.Level);
    }

    [Fact]
    public void JsonConverter_Deserialize_Null_ReturnsNull() {
        var result = JsonSerializer.Deserialize<MaterialContrastLevel?>("null");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"\"", "MaterialContrastLevel cannot be null or empty string.")]
    [InlineData("\"unknown\"", "Contrast level is invalid.")]
    [InlineData("2", "Invalid contrast level: '2'. Valid values are -1, 0, or 1.")]
    [InlineData("{}", "Contrast level is invalid.")]
    [InlineData("true", "Unexpected JSON payload for MaterialContrastLevel.")]
    public void JsonConverter_Deserialize_InvalidInput_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<MaterialContrastLevel?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void JsonConverter_Serialize_WritesExpectedJsonObject() {
        var json = JsonSerializer.Serialize(MaterialContrastLevel.High);

        Assert.Equal("{\"Level\":1}", json);
    }

    [Fact]
    public void JsonConverter_RoundTrip_ReturnsEquivalentLevel() {
        var level = MaterialContrastLevel.Reduced;

        var json = JsonSerializer.Serialize(level);
        var roundTrip = JsonSerializer.Deserialize<MaterialContrastLevel>(json);

        Assert.NotNull(roundTrip);
        Assert.Same(level, roundTrip);
    }

    [Fact]
    public void Operators_ImplicitConversion_WorkAsExpected() {
        MaterialContrastLevel? levelFromInt = -1;
        int? value = levelFromInt;

        Assert.Same(MaterialContrastLevel.Reduced, levelFromInt);
        Assert.Equal(-1, value);
    }

    [Fact]
    public void TypeConverter_CanConvertFromAndToString_ReturnsTrue() {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialContrastLevel));

        Assert.True(converter.CanConvertFrom(typeof(string)));
        Assert.True(converter.CanConvertTo(typeof(string)));
    }

    [Theory]
    [InlineData("-1", -1)]
    [InlineData("0", 0)]
    [InlineData("High", 1)]
    public void TypeConverter_ConvertFromString_ValidInput_ReturnsExpectedLevel(string input, int expectedLevel) {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialContrastLevel));

        var result = converter.ConvertFromInvariantString(input);

        var level = Assert.IsType<MaterialContrastLevel>(result);
        Assert.Equal(expectedLevel, level.Level);
    }

    [Fact]
    public void TypeConverter_ConvertToString_ReturnsExpectedValue() {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialContrastLevel));

        var result = converter.ConvertToInvariantString(MaterialContrastLevel.Reduced);

        Assert.Equal("-1", result);
    }

    [Fact]
    public void TypeConverter_ConvertFromString_InvalidInput_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialContrastLevel));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("99"));
    }

    [Fact]
    public void TypeConverter_ConvertFromString_WhitespaceOnly_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialContrastLevel));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("   "));
    }
}
