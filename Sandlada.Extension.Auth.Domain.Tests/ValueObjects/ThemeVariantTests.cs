using System.ComponentModel;
using System.Text.Json;
using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Tests.ValueObjects;

public sealed class ThemeVariantTests {
    [Theory]
    [InlineData((byte) 0, "Monochrome")]
    [InlineData((byte) 1, "Neutral")]
    [InlineData((byte) 2, "TonalSpot")]
    [InlineData((byte) 3, "Vibrant")]
    [InlineData((byte) 4, "Expressive")]
    [InlineData((byte) 5, "Fidelity")]
    [InlineData((byte) 6, "Content")]
    [InlineData((byte) 7, "Rainbow")]
    [InlineData((byte) 8, "FruitSalad")]
    public void From_ValidCode_ReturnsExpectedVariant(byte input, string expectedName) {
        var result = MaterialVariant.From(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, result.Value.Name);
        Assert.Equal(input, result.Value.Code);
        Assert.Same(ResolveVariant(expectedName), result.Value);
    }

    [Fact]
    public void From_InvalidCode_ReturnsExpectedFailure() {
        var result = MaterialVariant.From((byte) 9);

        Assert.True(result.IsFailure);
        Assert.Equal("MaterialVariant.InvalidCode", result.Error.Code);
        Assert.Equal("Invalid material variant code: '9'. Valid values are 0 to 8.", result.Error.Message);
    }

    [Theory]
    [InlineData("Monochrome", (byte) 0)]
    [InlineData(" neutral ", (byte) 1)]
    [InlineData("TONALSPOT", (byte) 2)]
    [InlineData("Vibrant", (byte) 3)]
    [InlineData("Expressive", (byte) 4)]
    [InlineData("Fidelity", (byte) 5)]
    [InlineData("Content", (byte) 6)]
    [InlineData("Rainbow", (byte) 7)]
    [InlineData("FruitSalad", (byte) 8)]
    public void From_ValidName_ReturnsExpectedVariant(string input, byte expectedCode) {
        var result = MaterialVariant.From(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedCode, result.Value.Code);
    }

    [Fact]
    public void From_InvalidName_ReturnsExpectedFailure() {
        var result = MaterialVariant.From("unknown");

        Assert.True(result.IsFailure);
        Assert.Equal("MaterialVariant.InvalidName", result.Error.Code);
        Assert.Equal("Invalid material variant name: 'unknown'. Valid values are 'Monochrome', 'Neutral', 'TonalSpot', 'Vibrant', 'Expressive', 'Fidelity', 'Content', 'Rainbow', and 'FruitSalad'.", result.Error.Message);
    }

    [Theory]
    [InlineData("3", (byte) 3)]
    [InlineData("Expressive", (byte) 4)]
    [InlineData(" fruitsalad ", (byte) 8)]
    public void Parse_ValidText_ReturnsExpectedVariant(string input, byte expectedCode) {
        var result = MaterialVariant.Parse(input);

        Assert.Equal(expectedCode, result.Code);
    }

    [Theory]
    [InlineData("{\"Name\":\"Vibrant\"}", (byte) 3)]
    [InlineData("{\"Code\":4}", (byte) 4)]
    [InlineData("{\"Name\":\"Content\",\"Code\":\"6\"}", (byte) 6)]
    public void Parse_ObjectText_ReturnsExpectedVariant(string input, byte expectedCode) {
        var result = MaterialVariant.Parse(input);

        Assert.Equal(expectedCode, result.Code);
    }

    [Fact]
    public void Parse_ObjectTextWithNameCodeMismatch_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => MaterialVariant.Parse("{\"Name\":\"Vibrant\",\"Code\":1}"));
    }

    [Fact]
    public void Parse_NullInput_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => MaterialVariant.Parse(null!));
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse() {
        var result = MaterialVariant.TryParse(null, null, out var variant);

        Assert.False(result);
        Assert.Null(variant);
    }

    [Theory]
    [InlineData("\"Vibrant\"", (byte) 3)]
    [InlineData("\"4\"", (byte) 4)]
    [InlineData("7", (byte) 7)]
    public void JsonConverter_Deserialize_ScalarInput_ReturnsExpectedVariant(string json, byte expectedCode) {
        var result = JsonSerializer.Deserialize<MaterialVariant?>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedCode, result!.Code);
    }

    [Theory]
    [InlineData("{\"Name\":\"Neutral\"}", (byte) 1)]
    [InlineData("{\"Code\":5}", (byte) 5)]
    [InlineData("{\"Name\":\"Rainbow\",\"Code\":7}", (byte) 7)]
    public void JsonConverter_Deserialize_ObjectInput_ReturnsExpectedVariant(string json, byte expectedCode) {
        var result = JsonSerializer.Deserialize<MaterialVariant?>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedCode, result!.Code);
    }

    [Fact]
    public void JsonConverter_Deserialize_Null_ReturnsNull() {
        var result = JsonSerializer.Deserialize<MaterialVariant?>("null");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("\"\"", "MaterialVariant cannot be null or empty string.")]
    [InlineData("\"unknown\"", "Material variant is invalid.")]
    [InlineData("9", "Invalid material variant code: '9'. Valid values are 0 to 8.")]
    [InlineData("{}", "Material variant is invalid.")]
    [InlineData("{\"Name\":\"Vibrant\",\"Code\":1}", "Material variant is invalid.")]
    [InlineData("true", "Unexpected JSON payload for MaterialVariant.")]
    public void JsonConverter_Deserialize_InvalidInput_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<MaterialVariant?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void JsonConverter_Serialize_WritesExpectedJsonObject() {
        var json = JsonSerializer.Serialize(MaterialVariant.Expressive);

        Assert.Equal("{\"Name\":\"Expressive\",\"Code\":4}", json);
    }

    [Fact]
    public void JsonConverter_RoundTrip_ReturnsEquivalentVariant() {
        var variant = MaterialVariant.Content;

        var json = JsonSerializer.Serialize(variant);
        var roundTrip = JsonSerializer.Deserialize<MaterialVariant>(json);

        Assert.NotNull(roundTrip);
        Assert.Same(variant, roundTrip);
    }

    [Fact]
    public void Operators_ImplicitConversion_WorkAsExpected() {
        MaterialVariant? fromString = "expressive";
        MaterialVariant? fromCode = (byte) 7;
        string? asString = fromString;
        byte? asCode = fromCode;

        Assert.Same(MaterialVariant.Expressive, fromString);
        Assert.Same(MaterialVariant.Rainbow, fromCode);
        Assert.Equal("Expressive", asString);
        Assert.Equal((byte) 7, asCode);
    }

    [Fact]
    public void TypeConverter_CanConvertFromAndToString_ReturnsTrue() {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialVariant));

        Assert.True(converter.CanConvertFrom(typeof(string)));
        Assert.True(converter.CanConvertTo(typeof(string)));
    }

    [Theory]
    [InlineData("Monochrome", (byte) 0)]
    [InlineData("8", (byte) 8)]
    [InlineData(" expressive ", (byte) 4)]
    public void TypeConverter_ConvertFromString_ValidInput_ReturnsExpectedVariant(string input, byte expectedCode) {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialVariant));

        var result = converter.ConvertFromInvariantString(input);

        var variant = Assert.IsType<MaterialVariant>(result);
        Assert.Equal(expectedCode, variant.Code);
    }

    [Fact]
    public void TypeConverter_ConvertToString_ReturnsVariantName() {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialVariant));

        var result = converter.ConvertToInvariantString(MaterialVariant.TonalSpot);

        Assert.Equal("TonalSpot", result);
    }

    [Fact]
    public void TypeConverter_ConvertFromString_InvalidInput_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialVariant));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("99"));
    }

    [Fact]
    public void TypeConverter_ConvertFromString_WhitespaceOnly_ThrowsArgumentException() {
        var converter = TypeDescriptor.GetConverter(typeof(MaterialVariant));

        Assert.Throws<ArgumentException>(() => converter.ConvertFromInvariantString("   "));
    }

    private static MaterialVariant ResolveVariant(string name) {
        return name switch {
            "Monochrome" => MaterialVariant.Monochrome,
            "Neutral" => MaterialVariant.Neutral,
            "TonalSpot" => MaterialVariant.TonalSpot,
            "Vibrant" => MaterialVariant.Vibrant,
            "Expressive" => MaterialVariant.Expressive,
            "Fidelity" => MaterialVariant.Fidelity,
            "Content" => MaterialVariant.Content,
            "Rainbow" => MaterialVariant.Rainbow,
            "FruitSalad" => MaterialVariant.FruitSalad,
            _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown variant name.")
        };
    }
}
