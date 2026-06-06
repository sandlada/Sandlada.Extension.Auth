using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.ValueObjects;

public sealed record MaterialContrastLevelConstructorArgs {
    public required int Level { get; init; }
}

[JsonConverter(typeof(MaterialContrastLevelJsonConverter))]
[TypeConverter(typeof(MaterialContrastLevelTypeConverter))]
public sealed record MaterialContrastLevel : IEquatable<MaterialContrastLevel>, IParsable<MaterialContrastLevel> {

    #region Enums
    public static readonly MaterialContrastLevel Reduced = new(-1);
    public static readonly MaterialContrastLevel Default = new(0);
    public static readonly MaterialContrastLevel High    = new(1);
    #endregion

    #region Properties
    public int Level { get; private init; }
    #endregion

    #region Constructors
    private MaterialContrastLevel() {
    }
    private MaterialContrastLevel(int level) {
        this.Level = level;
    }
    public static IResult<MaterialContrastLevel> From(MaterialContrastLevelConstructorArgs args) {
        if (args is null) throw new ArgumentNullException(nameof(args));

        return args.Level switch {
            -1 => Result.Success(Reduced),
            0 => Result.Success(Default),
            1 => Result.Success(High),
            _ => Result.Failure<MaterialContrastLevel>(DomainError.MaterialContrastLevel.InvalidLevel(args.Level)),
        };
    }
    #endregion

    #region Serialization
    public static MaterialContrastLevel Parse(string s, IFormatProvider? provider = null) {
        if (TryParse(s, provider, out var result)) return result;
        throw new ArgumentException($"Invalid MaterialContrastLevel: '{s}'.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MaterialContrastLevel result) {
        result = default!;

        if (string.IsNullOrWhiteSpace(s)) return false;

        var trimmed = s.Trim();
        if (LooksLikeJsonObjectText(trimmed)) {
            return TryParseJsonObjectText(trimmed, out result);
        }

        if (TryParseNamedLevel(trimmed, out result)) return true;

        if (!int.TryParse(trimmed, NumberStyles.Integer, provider as CultureInfo ?? CultureInfo.InvariantCulture, out var parsedLevel)) {
            return false;
        }

        var creationResult = From(new MaterialContrastLevelConstructorArgs { Level = parsedLevel });
        if (creationResult.IsFailure) return false;

        result = creationResult.Value;
        return true;
    }

    public sealed class MaterialContrastLevelJsonConverter : JsonConverter<MaterialContrastLevel> {
        public override MaterialContrastLevel? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.Number) {
                if (!reader.TryGetInt32(out var levelValue)) throw new JsonException("MaterialContrastLevel number value is invalid.");

                var creationResult = From(new MaterialContrastLevelConstructorArgs { Level = levelValue });
                if (creationResult.IsFailure) throw new JsonException(creationResult.Error.Message);
                return creationResult.Value;
            }

            if (reader.TokenType == JsonTokenType.String) {
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value)) throw new JsonException("MaterialContrastLevel cannot be null or empty string.");

                var trimmed = value.Trim();
                if (LooksLikeJsonObjectText(trimmed) && TryParseJsonObjectText(trimmed, out var parsedFromObject)) return parsedFromObject;
                if (TryParse(trimmed, CultureInfo.InvariantCulture, out var parsedFromString)) return parsedFromString;

                throw new JsonException(DomainError.MaterialContrastLevel.Invalid.Message);
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                var creationResult = ReadFromObject(ref reader);
                if (creationResult.IsFailure) throw new JsonException(creationResult.Error.Message);
                return creationResult.Value;
            }

            throw new JsonException("Unexpected JSON payload for MaterialContrastLevel.");
        }

        public override void Write(Utf8JsonWriter writer, MaterialContrastLevel value, JsonSerializerOptions options) {
            if (value is null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteNumber(nameof(MaterialContrastLevel.Level), value.Level);
            writer.WriteEndObject();
        }
    }

    public sealed class MaterialContrastLevelTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            if (value is string strValue) {
                if (TryParse(strValue, culture, out var parsed)) return parsed;
                throw new ArgumentException($"Invalid MaterialContrastLevel: '{strValue}'.");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
            if (value is MaterialContrastLevel level && destinationType == typeof(string)) return level.Level.ToString(CultureInfo.InvariantCulture);
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public static implicit operator int?(MaterialContrastLevel? contrastLevel) => contrastLevel?.Level;
    public static implicit operator MaterialContrastLevel?(int? value) {
        if (!value.HasValue) return null;

        var creationResult = From(new MaterialContrastLevelConstructorArgs { Level = value.Value });
        return creationResult.IsSuccess ? creationResult.Value : null;
    }
    #endregion

    public override string ToString() => this.Level.ToString(CultureInfo.InvariantCulture);

    #region Utils
    private static bool LooksLikeJsonObjectText(string value) => value.StartsWith('{') && value.EndsWith('}');

    private static bool TryParseNamedLevel(string value, [MaybeNullWhen(false)] out MaterialContrastLevel result) {
        result = default!;

        if (string.Equals(value, nameof(Reduced), StringComparison.OrdinalIgnoreCase)) {
            result = Reduced;
            return true;
        }

        if (string.Equals(value, nameof(Default), StringComparison.OrdinalIgnoreCase)) {
            result = Default;
            return true;
        }

        if (string.Equals(value, nameof(High), StringComparison.OrdinalIgnoreCase)) {
            result = High;
            return true;
        }

        return false;
    }

    private static bool TryParseJsonObjectText(string value, [MaybeNullWhen(false)] out MaterialContrastLevel result) {
        result = default!;

        try {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;

            foreach (var property in document.RootElement.EnumerateObject()) {
                if (!string.Equals(property.Name, nameof(MaterialContrastLevel.Level), StringComparison.OrdinalIgnoreCase)) continue;

                if (property.Value.ValueKind == JsonValueKind.Number) {
                    if (!property.Value.TryGetInt32(out var numericLevel)) return false;

                    var numericResult = From(new MaterialContrastLevelConstructorArgs { Level = numericLevel });
                    if (numericResult.IsFailure) return false;

                    result = numericResult.Value;
                    return true;
                }

                if (property.Value.ValueKind == JsonValueKind.String) {
                    var raw = property.Value.GetString();
                    if (!TryParse(raw, CultureInfo.InvariantCulture, out result)) return false;
                    return true;
                }

                return false;
            }

            return false;
        } catch {
            return false;
        }
    }

    private static IResult<MaterialContrastLevel> ReadFromObject(ref Utf8JsonReader reader) {
        int? numericLevel = null;
        string? textualLevel = null;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            if (!reader.Read()) break;

            if (!string.Equals(propertyName, nameof(MaterialContrastLevel.Level), StringComparison.OrdinalIgnoreCase)) continue;

            if (reader.TokenType == JsonTokenType.Number) {
                if (!reader.TryGetInt32(out var parsedNumber)) return Result.Failure<MaterialContrastLevel>(DomainError.MaterialContrastLevel.Invalid);
                numericLevel = parsedNumber;
            } else if (reader.TokenType == JsonTokenType.String) {
                textualLevel = reader.GetString();
            } else {
                return Result.Failure<MaterialContrastLevel>(DomainError.MaterialContrastLevel.Invalid);
            }
        }

        if (numericLevel.HasValue) {
            return From(new MaterialContrastLevelConstructorArgs { Level = numericLevel.Value });
        }

        if (!string.IsNullOrWhiteSpace(textualLevel) && TryParse(textualLevel, CultureInfo.InvariantCulture, out var parsedTextualLevel)) {
            return Result.Success(parsedTextualLevel);
        }

        return Result.Failure<MaterialContrastLevel>(DomainError.MaterialContrastLevel.Invalid);
    }
    #endregion

}
