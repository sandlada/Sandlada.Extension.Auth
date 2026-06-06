using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.ValueObjects;

[JsonConverter(typeof(MaterialVariantJsonConverter))]
[TypeConverter(typeof(MaterialVariantTypeConverter))]
public sealed record MaterialVariant : IEquatable<MaterialVariant>, IParsable<MaterialVariant> {

    #region Enums
    public static readonly MaterialVariant Monochrome = new("Monochrome", 0);
    public static readonly MaterialVariant Neutral    = new ("Neutral", 1);
    public static readonly MaterialVariant TonalSpot  = new ("TonalSpot", 2);
    public static readonly MaterialVariant Vibrant    = new ("Vibrant", 3);
    public static readonly MaterialVariant Expressive = new ("Expressive", 4);
    public static readonly MaterialVariant Fidelity   = new ("Fidelity", 5);
    public static readonly MaterialVariant Content    = new ("Content", 6);
    public static readonly MaterialVariant Rainbow    = new ("Rainbow", 7);
    public static readonly MaterialVariant FruitSalad = new ("FruitSalad", 8);
    #endregion

    #region Properties
    public string Name { get; private init; } = string.Empty;
    public byte Code { get; private init; }
    #endregion

    #region Constructors
    private MaterialVariant() {
    }

    private MaterialVariant(string name, byte code) {
        this.Name = name;
        this.Code = code;
    }

    public static IResult<MaterialVariant> From(byte args) {
        return args switch {
            0 => Result.Success(Monochrome),
            1 => Result.Success(Neutral),
            2 => Result.Success(TonalSpot),
            3 => Result.Success(Vibrant),
            4 => Result.Success(Expressive),
            5 => Result.Success(Fidelity),
            6 => Result.Success(Content),
            7 => Result.Success(Rainbow),
            8 => Result.Success(FruitSalad),
            _ => Result.Failure<MaterialVariant>(DomainError.MaterialVariant.InvalidCode(args)),
        };
    }

    public static IResult<MaterialVariant> From(string args) {
        if (string.IsNullOrWhiteSpace(args)) return Result.Failure<MaterialVariant>(DomainError.MaterialVariant.InvalidName(args ?? string.Empty));

        var normalized = args.Trim();
        if (TryParseNamedVariant(normalized, out var variant)) return Result.Success(variant);

        return Result.Failure<MaterialVariant>(DomainError.MaterialVariant.InvalidName(args));
    }
    #endregion

    #region Serialization
    public static MaterialVariant Parse(string s, IFormatProvider? provider = null) {
        if (TryParse(s, provider, out var result)) return result;
        throw new ArgumentException($"Invalid MaterialVariant: '{s}'.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out MaterialVariant result) {
        result = default!;

        if (string.IsNullOrWhiteSpace(s)) return false;

        var trimmed = s.Trim();
        if (LooksLikeJsonObjectText(trimmed)) {
            return TryParseJsonObjectText(trimmed, out result);
        }

        if (byte.TryParse(trimmed, NumberStyles.Integer, provider as CultureInfo ?? CultureInfo.InvariantCulture, out var code)) {
            var fromCodeResult = From(code);
            if (fromCodeResult.IsFailure) return false;

            result = fromCodeResult.Value;
            return true;
        }

        var fromNameResult = From(trimmed);
        if (fromNameResult.IsFailure) return false;

        result = fromNameResult.Value;
        return true;
    }

    public sealed class MaterialVariantJsonConverter : JsonConverter<MaterialVariant> {
        public override MaterialVariant? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.Number) {
                if (!reader.TryGetByte(out var codeValue)) throw new JsonException("MaterialVariant code value is invalid.");

                var creationResult = From(codeValue);
                if (creationResult.IsFailure) throw new JsonException(creationResult.Error.Message);
                return creationResult.Value;
            }

            if (reader.TokenType == JsonTokenType.String) {
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value)) throw new JsonException("MaterialVariant cannot be null or empty string.");

                var trimmed = value.Trim();
                if (LooksLikeJsonObjectText(trimmed) && TryParseJsonObjectText(trimmed, out var parsedFromObject)) return parsedFromObject;
                if (TryParse(trimmed, CultureInfo.InvariantCulture, out var parsedFromString)) return parsedFromString;

                throw new JsonException(DomainError.MaterialVariant.Invalid.Message);
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                var creationResult = ReadFromObject(ref reader);
                if (creationResult.IsFailure) throw new JsonException(creationResult.Error.Message);
                return creationResult.Value;
            }

            throw new JsonException("Unexpected JSON payload for MaterialVariant.");
        }

        public override void Write(Utf8JsonWriter writer, MaterialVariant value, JsonSerializerOptions options) {
            if (value is null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteString(nameof(MaterialVariant.Name), value.Name);
            writer.WriteNumber(nameof(MaterialVariant.Code), value.Code);
            writer.WriteEndObject();
        }
    }

    public sealed class MaterialVariantTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            if (value is string strValue) {
                if (TryParse(strValue, culture, out var parsed)) return parsed;
                throw new ArgumentException($"Invalid MaterialVariant: '{strValue}'.");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
            if (value is MaterialVariant variant && destinationType == typeof(string)) return variant.Name;
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
    #endregion

    public static implicit operator string?(MaterialVariant? themeVariant) => themeVariant?.Name;
    public static implicit operator byte?(MaterialVariant? themeVariant) => themeVariant?.Code;
    public static implicit operator MaterialVariant?(string? value) {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Parse(value);
    }
    public static implicit operator MaterialVariant?(byte? value) {
        if (!value.HasValue) return null;

        var fromCodeResult = From(value.Value);
        return fromCodeResult.IsSuccess ? fromCodeResult.Value : null;
    }

    public override string ToString() => this.Name;

    #region Utils
    private static bool LooksLikeJsonObjectText(string value) => value.StartsWith('{') && value.EndsWith('}');

    private static bool TryParseNamedVariant(string value, [MaybeNullWhen(false)] out MaterialVariant result) {
        result = default!;

        if (string.Equals(value, Monochrome.Name, StringComparison.OrdinalIgnoreCase)) {
            result = Monochrome;
            return true;
        }

        if (string.Equals(value, Neutral.Name, StringComparison.OrdinalIgnoreCase)) {
            result = Neutral;
            return true;
        }

        if (string.Equals(value, TonalSpot.Name, StringComparison.OrdinalIgnoreCase)) {
            result = TonalSpot;
            return true;
        }

        if (string.Equals(value, Vibrant.Name, StringComparison.OrdinalIgnoreCase)) {
            result = Vibrant;
            return true;
        }

        if (string.Equals(value, Expressive.Name, StringComparison.OrdinalIgnoreCase)) {
            result = Expressive;
            return true;
        }

        if (string.Equals(value, Fidelity.Name, StringComparison.OrdinalIgnoreCase)) {
            result = Fidelity;
            return true;
        }

        if (string.Equals(value, Content.Name, StringComparison.OrdinalIgnoreCase)) {
            result = Content;
            return true;
        }

        if (string.Equals(value, Rainbow.Name, StringComparison.OrdinalIgnoreCase)) {
            result = Rainbow;
            return true;
        }

        if (string.Equals(value, FruitSalad.Name, StringComparison.OrdinalIgnoreCase)) {
            result = FruitSalad;
            return true;
        }

        return false;
    }

    private static bool TryParseJsonObjectText(string value, [MaybeNullWhen(false)] out MaterialVariant result) {
        result = default!;

        try {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;

            string? name = null;
            byte? code = null;
            string? codeText = null;

            foreach (var property in document.RootElement.EnumerateObject()) {
                if (string.Equals(property.Name, nameof(MaterialVariant.Name), StringComparison.OrdinalIgnoreCase)) {
                    if (property.Value.ValueKind != JsonValueKind.String) return false;
                    name = property.Value.GetString();
                    continue;
                }

                if (!string.Equals(property.Name, nameof(MaterialVariant.Code), StringComparison.OrdinalIgnoreCase)) continue;

                if (property.Value.ValueKind == JsonValueKind.Number) {
                    if (!property.Value.TryGetByte(out var parsedCode)) return false;
                    code = parsedCode;
                } else if (property.Value.ValueKind == JsonValueKind.String) {
                    codeText = property.Value.GetString();
                } else {
                    return false;
                }
            }

            if (!code.HasValue && !string.IsNullOrWhiteSpace(codeText)) {
                if (!byte.TryParse(codeText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedCodeText)) return false;
                code = parsedCodeText;
            }

            if (!string.IsNullOrWhiteSpace(name) && code.HasValue) {
                var fromNameResult = From(name);
                var fromCodeResult = From(code.Value);

                if (fromNameResult.IsFailure || fromCodeResult.IsFailure) return false;
                if (fromNameResult.Value.Code != fromCodeResult.Value.Code) return false;

                result = fromCodeResult.Value;
                return true;
            }

            if (!string.IsNullOrWhiteSpace(name)) {
                var fromNameResult = From(name);
                if (fromNameResult.IsFailure) return false;

                result = fromNameResult.Value;
                return true;
            }

            if (code.HasValue) {
                var fromCodeResult = From(code.Value);
                if (fromCodeResult.IsFailure) return false;

                result = fromCodeResult.Value;
                return true;
            }

            return false;
        } catch {
            return false;
        }
    }

    private static IResult<MaterialVariant> ReadFromObject(ref Utf8JsonReader reader) {
        string? name = null;
        byte? code = null;
        string? codeText = null;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            if (!reader.Read()) break;

            if (string.Equals(propertyName, nameof(MaterialVariant.Name), StringComparison.OrdinalIgnoreCase)) {
                if (reader.TokenType != JsonTokenType.String) return Result.Failure<MaterialVariant>(DomainError.MaterialVariant.Invalid);
                name = reader.GetString();
                continue;
            }

            if (!string.Equals(propertyName, nameof(MaterialVariant.Code), StringComparison.OrdinalIgnoreCase)) continue;

            if (reader.TokenType == JsonTokenType.Number) {
                if (!reader.TryGetByte(out var parsedCode)) return Result.Failure<MaterialVariant>(DomainError.MaterialVariant.Invalid);
                code = parsedCode;
            } else if (reader.TokenType == JsonTokenType.String) {
                codeText = reader.GetString();
            } else {
                return Result.Failure<MaterialVariant>(DomainError.MaterialVariant.Invalid);
            }
        }

        if (!code.HasValue && !string.IsNullOrWhiteSpace(codeText)) {
            if (!byte.TryParse(codeText.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedCodeText)) {
                return Result.Failure<MaterialVariant>(DomainError.MaterialVariant.Invalid);
            }
            code = parsedCodeText;
        }

        if (!string.IsNullOrWhiteSpace(name) && code.HasValue) {
            var fromNameResult = From(name);
            if (fromNameResult.IsFailure) return fromNameResult;

            var fromCodeResult = From(code.Value);
            if (fromCodeResult.IsFailure) return fromCodeResult;

            if (fromNameResult.Value.Code != fromCodeResult.Value.Code) {
                return Result.Failure<MaterialVariant>(DomainError.MaterialVariant.Invalid);
            }

            return Result.Success(fromCodeResult.Value);
        }

        if (!string.IsNullOrWhiteSpace(name)) {
            return From(name);
        }

        if (code.HasValue) {
            return From(code.Value);
        }

        return Result.Failure<MaterialVariant>(DomainError.MaterialVariant.Invalid);
    }
    #endregion

}
