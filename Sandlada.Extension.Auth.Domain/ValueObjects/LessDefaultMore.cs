using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sandlada.Extension.Auth.Domain.ValueObjects;

[JsonConverter(typeof(LessDefaultMoreJsonConverter))]
[TypeConverter(typeof(LessDefaultMoreTypeConverter))]
public sealed record LessDefaultMore : IEquatable<LessDefaultMore>, IParsable<LessDefaultMore> {

    public const string LessString = "Less";
    public const string DefaultString = "Default";
    public const string MoreString = "More";

    public static readonly LessDefaultMore Less = new(LessString);
    public static readonly LessDefaultMore Default = new(DefaultString);
    public static readonly LessDefaultMore More = new(MoreString);

    public string Value { get; private init; } = string.Empty;

    private LessDefaultMore() {
    }

    private LessDefaultMore(string value) {
        this.Value = value;
    }

    public static LessDefaultMore Parse(string s, IFormatProvider? provider = null) {
        if (TryParse(s, provider, out var result)) return result;
        throw new ArgumentException($"Invalid LessDefaultMore: '{s}'.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out LessDefaultMore result) {
        result = default!;

        if (string.IsNullOrWhiteSpace(s)) return false;

        var trimmed = s.Trim();
        if (LooksLikeJsonObjectText(trimmed)) {
            return TryParseJsonObjectText(trimmed, out result);
        }

        return TryParseKnownValue(trimmed, out result);
    }

    public sealed class LessDefaultMoreJsonConverter : JsonConverter<LessDefaultMore> {
        public override LessDefaultMore? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.String) {
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value)) throw new JsonException("LessDefaultMore cannot be null or empty string.");

                var trimmed = value.Trim();
                if (LooksLikeJsonObjectText(trimmed) && TryParseJsonObjectText(trimmed, out var parsedFromObject)) return parsedFromObject;
                if (TryParseKnownValue(trimmed, out var parsedFromString)) return parsedFromString;

                throw new JsonException("Invalid LessDefaultMore value.");
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                var parsed = ReadFromObject(ref reader);
                if (parsed is not null) return parsed;

                throw new JsonException("Invalid LessDefaultMore value.");
            }

            throw new JsonException("Unexpected JSON payload for LessDefaultMore.");
        }

        public override void Write(Utf8JsonWriter writer, LessDefaultMore value, JsonSerializerOptions options) {
            if (value is null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteString(nameof(LessDefaultMore.Value), value.Value);
            writer.WriteEndObject();
        }
    }

    public sealed class LessDefaultMoreTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            if (value is string strValue) {
                if (TryParse(strValue, culture, out var parsed)) return parsed;
                throw new ArgumentException($"Invalid LessDefaultMore: '{strValue}'. Valid values are 'Less', 'Default', or 'More'.");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
            if (value is LessDefaultMore lessDefaultMore && destinationType == typeof(string)) return lessDefaultMore.Value;
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public static implicit operator string?(LessDefaultMore? value) => value?.Value;
    public static implicit operator LessDefaultMore?(string? value) {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Parse(value);
    }

    public override string ToString() => this.Value;

    private static bool LooksLikeJsonObjectText(string value) => value.StartsWith('{') && value.EndsWith('}');

    private static bool TryParseKnownValue(string value, [MaybeNullWhen(false)] out LessDefaultMore result) {
        result = default!;

        if (string.Equals(value, LessString, StringComparison.OrdinalIgnoreCase)) {
            result = Less;
            return true;
        }

        if (string.Equals(value, DefaultString, StringComparison.OrdinalIgnoreCase)) {
            result = Default;
            return true;
        }

        if (string.Equals(value, MoreString, StringComparison.OrdinalIgnoreCase)) {
            result = More;
            return true;
        }

        return false;
    }

    private static bool TryParseJsonObjectText(string value, [MaybeNullWhen(false)] out LessDefaultMore result) {
        result = default!;

        try {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;

            foreach (var property in document.RootElement.EnumerateObject()) {
                if (!string.Equals(property.Name, nameof(LessDefaultMore.Value), StringComparison.OrdinalIgnoreCase)) continue;
                if (property.Value.ValueKind != JsonValueKind.String) return false;

                var rawValue = property.Value.GetString();
                if (string.IsNullOrWhiteSpace(rawValue)) return false;

                return TryParseKnownValue(rawValue.Trim(), out result);
            }

            return false;
        } catch {
            return false;
        }
    }

    private static LessDefaultMore? ReadFromObject(ref Utf8JsonReader reader) {
        string? value = null;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            if (!reader.Read()) break;

            if (!string.Equals(propertyName, nameof(LessDefaultMore.Value), StringComparison.OrdinalIgnoreCase)) continue;
            if (reader.TokenType != JsonTokenType.String) return null;

            value = reader.GetString();
        }

        if (string.IsNullOrWhiteSpace(value)) return null;
        return TryParseKnownValue(value.Trim(), out var parsed) ? parsed : null;
    }

}
