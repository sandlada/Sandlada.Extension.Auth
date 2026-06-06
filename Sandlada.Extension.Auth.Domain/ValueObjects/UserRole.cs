using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.ValueObjects;

[JsonConverter(typeof(UserRoleJsonConverter))]
[TypeConverter(typeof(UserRoleTypeConverter))]
public sealed record UserRole : IEquatable<UserRole>, IParsable<UserRole> {
    #region Constants for API Authorization
    public const string AdministratorString = "administrator";
    public const string NormalString = "normal";
    #endregion

    #region Properties for Common Roles
    public static readonly UserRole Administrator = new(AdministratorString);
    public static readonly UserRole Normal = new(NormalString);
    #endregion

    #region Properties
    public string Value { get; private init; }
    #endregion

    #region Constructors
    private UserRole(string value) {
        this.Value = value switch {
            AdministratorString => AdministratorString,
            NormalString => NormalString,
            _ => NormalString,
        };
    }

    public static IResult<UserRole> From(string value) {
        if (string.IsNullOrWhiteSpace(value)) return Result.Failure<UserRole>(DomainError.UserRole.InvalidValue(value ?? string.Empty));

        return value.Trim().ToLowerInvariant() switch {
            AdministratorString => Result.Success(Administrator),
            NormalString => Result.Success(Normal),
            _ => Result.Failure<UserRole>(DomainError.UserRole.InvalidValue(value)),
        };
    }
    #endregion

    #region Convertors
    public sealed class UserRoleJsonConverter : JsonConverter<UserRole> {
        public override UserRole? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.String) {
                var str = reader.GetString();
                if (string.IsNullOrWhiteSpace(str)) throw new JsonException("UserRole cannot be null or empty string.");

                var result = From(str);
                if (result.IsFailure) throw new JsonException(result.Error.Message);
                return result.Value;
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                var result = ReadFromObject(ref reader);
                if (result.IsFailure) throw new JsonException(result.Error.Message);
                return result.Value;
            }

            throw new JsonException("Unexpected JSON payload for UserRole.");
        }

        public override void Write(Utf8JsonWriter writer, UserRole value, JsonSerializerOptions options) {
            if (value is null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteString(nameof(UserRole.Value), value.Value);
            writer.WriteEndObject();
        }
    }

    public sealed class UserRoleTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            if (value is string strValue) {
                if (TryParse(strValue, culture, out var parsed)) return parsed;
                throw new ArgumentException($"Invalid UserRole: '{strValue}'. Valid values are 'Administrator' or 'Normal'.");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
            if (value is UserRole role && destinationType == typeof(string)) return role.Value;
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    public static UserRole Parse(string s, IFormatProvider? provider = null) {
        if (TryParse(s, provider, out var result)) return result;
        throw new ArgumentException($"Invalid UserRole: '{s}'.");
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out UserRole result) {
        if (string.IsNullOrWhiteSpace(s)) {
            result = null;
            return false;
        }

        var trimmed = s.Trim();
        if (LooksLikeJsonObjectText(trimmed)) {
            return TryParseJsonObjectText(trimmed, out result);
        }

        var parseResult = From(trimmed);
        if (parseResult.IsFailure) {
            result = null;
            return false;
        }

        result = parseResult.Value;
        return true;
    }
    #endregion

    public static implicit operator string?(UserRole? role) => role?.Value;
    public static implicit operator UserRole?(string? value) {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Parse(value);
    }

    public override string ToString() => this.Value;

    private static bool LooksLikeJsonObjectText(string value) => value.StartsWith('{') && value.EndsWith('}');

    private static bool TryParseJsonObjectText(string value, out UserRole? result) {
        result = null;

        try {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;

            foreach (var property in document.RootElement.EnumerateObject()) {
                if (!string.Equals(property.Name, nameof(UserRole.Value), StringComparison.OrdinalIgnoreCase)) continue;

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

    private static IResult<UserRole> ReadFromObject(ref Utf8JsonReader reader) {
        string? value = null;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            if (!reader.Read()) break;

            if (string.Equals(propertyName, nameof(UserRole.Value), StringComparison.OrdinalIgnoreCase)) {
                value = reader.GetString();
            }
        }

        return From(value ?? string.Empty);
    }
}
