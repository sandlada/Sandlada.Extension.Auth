using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.ValueObjects;

public sealed record GenderConstructorArgs {
    public required string Value { get; init; }
}

[JsonConverter(typeof(GenderJsonConverter))]
[TypeConverter(typeof(GenderTypeConverter))]
public sealed record Gender : IEquatable<Gender>, IParsable<Gender> {
    #region Constants
    public const int MinCustomLength = 2;
    public const int MaxCustomLength = 16;
    public const string MaleString = "male";
    public const string FemaleString = "female";
    public const string UnknownString = "unknown";
    #endregion

    #region Properties for Builtin Options
    public static readonly Gender Male = new(MaleString);
    public static readonly Gender Female = new(FemaleString);
    public static readonly Gender Unknown = new(UnknownString);
    #endregion

    #region Properties
    public string Value { get; private init; }
    #endregion

    #region Constructors
    private Gender(string value) {
        this.Value = value;
    }

    private Gender(GenderConstructorArgs args) {
        this.Value = args.Value;
    }

    public static IResult<Gender> From(string value) {
        return From(new GenderConstructorArgs {
            Value = value ?? string.Empty,
        });
    }

    public static IResult<Gender> From(GenderConstructorArgs args) {
        if (args is null) return Result.Failure<Gender>(DomainError.Gender.Empty);
        if (string.IsNullOrWhiteSpace(args.Value)) return Result.Failure<Gender>(DomainError.Gender.Empty);

        var normalized = args.Value.Trim();
        if (string.Equals(normalized, MaleString, StringComparison.OrdinalIgnoreCase)) return Result.Success(Male);
        if (string.Equals(normalized, FemaleString, StringComparison.OrdinalIgnoreCase)) return Result.Success(Female);
        if (string.Equals(normalized, UnknownString, StringComparison.OrdinalIgnoreCase)) return Result.Success(Unknown);
        
        if (normalized.Length < MinCustomLength) return Result.Failure<Gender>(DomainError.Gender.ValueTooShort);
        if (normalized.Length > MaxCustomLength) return Result.Failure<Gender>(DomainError.Gender.ValueTooLong);

        return Result.Success(new Gender(new GenderConstructorArgs {
            Value = normalized,
        }));
    }
    #endregion

    #region Convertors
    public sealed class GenderJsonConverter : JsonConverter<Gender> {
        public override Gender? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.String) {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str)) throw new JsonException("Gender cannot be null or empty string.");

                var result = From(str);
                if (result.IsFailure) throw new JsonException(result.Error.Message);
                return result.Value;
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                var result = ReadFromObject(ref reader);
                if (result.IsFailure) throw new JsonException(result.Error.Message);
                return result.Value;
            }

            throw new JsonException("Unexpected JSON payload for Gender.");
        }

        public override void Write(Utf8JsonWriter writer, Gender value, JsonSerializerOptions options) {
            if (value is null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteString(nameof(Gender.Value), value.Value);
            writer.WriteEndObject();
        }
    }

    public sealed class GenderTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            if (value is string strValue) {
                if (TryParse(strValue, culture, out var parsed)) return parsed;
                throw new ArgumentException($"Invalid Gender: '{strValue}'.");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
            if (value is Gender gender && destinationType == typeof(string)) return gender.Value;
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public static Gender Parse(string s, IFormatProvider? provider = null) {
        if (TryParse(s, provider, out var result)) return result;
        throw new ArgumentException($"Invalid Gender: '{s}'.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Gender result) {
        result = default!;

        if (string.IsNullOrWhiteSpace(s)) return false;

        var trimmed = s.Trim();
        if (LooksLikeJsonObjectText(trimmed)) {
            return TryParseJsonObjectText(trimmed, out result);
        }

        var parseResult = From(trimmed);
        if (parseResult.IsFailure) return false;

        result = parseResult.Value;
        return true;
    }
    #endregion

    #region Operators and Overrides
    public static implicit operator string?(Gender? gender) => gender?.Value;
    public static implicit operator Gender?(string? value) {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Parse(value);
    }

    public override string ToString() => this.Value;
    #endregion

    #region Helpers
    private static bool LooksLikeJsonObjectText(string value) => value.StartsWith('{') && value.EndsWith('}');

    private static bool TryParseJsonObjectText(string value, [MaybeNullWhen(false)] out Gender result) {
        result = default!;

        try {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;

            foreach (var property in document.RootElement.EnumerateObject()) {
                if (!string.Equals(property.Name, nameof(Gender.Value), StringComparison.OrdinalIgnoreCase)) continue;
                if (property.Value.ValueKind != JsonValueKind.String) return false;

                var parseResult = From(property.Value.GetString() ?? string.Empty);
                if (parseResult.IsFailure) return false;

                result = parseResult.Value;
                return true;
            }

            return false;
        } catch {
            return false;
        }
    }

    private static IResult<Gender> ReadFromObject(ref Utf8JsonReader reader) {
        string? value = null;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            if (!reader.Read()) break;

            if (string.Equals(propertyName, nameof(Gender.Value), StringComparison.OrdinalIgnoreCase)) {
                value = reader.GetString();
            }
        }

        return From(value ?? string.Empty);
    }
    #endregion
}
