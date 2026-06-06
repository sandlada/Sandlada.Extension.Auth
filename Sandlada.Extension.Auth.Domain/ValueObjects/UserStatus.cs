
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.ValueObjects;

public sealed record UserStatusConstructorArgs {
    public required string Code { get; init; }
}

[JsonConverter(typeof(UserStatusJsonConverter))]
[TypeConverter(typeof(UserStatusTypeConverter))]
public sealed record UserStatus : IParsable<UserStatus>, IEquatable<UserStatus> {

    public static readonly UserStatus Disabled = new("Disabled");
    public static readonly UserStatus Enabled = new("Enabled");
    public static readonly UserStatus Suspended = new("Suspended");
    public static readonly UserStatus Blocked = new("Blocked");
    public static readonly UserStatus Deleted = new("Deleted");


    #region Properties
    public string Code { get; init; } = string.Empty;
    #endregion

    #region Constructors
    private UserStatus() {
    }
    private UserStatus(string code) {
        this.Code = code;
    }
    private UserStatus(UserStatusConstructorArgs args) {
        this.Code = args.Code;
    }

    public static IResult<UserStatus> From(UserStatusConstructorArgs args) {
        if (args is null) throw new ArgumentNullException(nameof(args));

        if (string.IsNullOrWhiteSpace(args.Code)) return Result.Failure<UserStatus>(DomainError.UserStatus.InvalidStatus);

        var code = args.Code.Trim();

        if (string.Equals(code, Disabled.Code, StringComparison.OrdinalIgnoreCase)) return Result.Success(Disabled);
        if (string.Equals(code, Enabled.Code, StringComparison.OrdinalIgnoreCase)) return Result.Success(Enabled);
        if (string.Equals(code, Suspended.Code, StringComparison.OrdinalIgnoreCase)) return Result.Success(Suspended);
        if (string.Equals(code, Blocked.Code, StringComparison.OrdinalIgnoreCase)) return Result.Success(Blocked);
        if (string.Equals(code, Deleted.Code, StringComparison.OrdinalIgnoreCase)) return Result.Success(Deleted);

        return Result.Failure<UserStatus>(DomainError.UserStatus.InvalidStatus);
    }
    #endregion

    #region Methods: IParsable

    public static UserStatus Parse(string s, IFormatProvider? provider = null) {
        if (!TryParse(s, provider, out var result)) throw new ArgumentException($"Invalid UserStatus: '{s}'.");
        return result;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out UserStatus result) {
        result = default!;
        if (s is null) return false;

        var trimmed = s.Trim();
        if (string.IsNullOrWhiteSpace(trimmed)) return false;

        if (LooksLikeJsonObjectText(trimmed)) {
            return TryParseJsonObjectText(trimmed, out result);
        }

        var creationResult = From(new UserStatusConstructorArgs { Code = trimmed });
        if (creationResult.IsFailure) return false;

        result = creationResult.Value;
        return true;
    }

    #endregion

    #region Methods: JSON

    public sealed class UserStatusJsonConverter : JsonConverter<UserStatus> {
        public override UserStatus? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.String) {
                var value = reader.GetString();
                if (string.IsNullOrWhiteSpace(value)) throw new JsonException("UserStatus cannot be null or empty string.");

                var result = From(new UserStatusConstructorArgs { Code = value });
                if (result.IsFailure) throw new JsonException(result.Error.Message);
                return result.Value;
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                var result = ReadFromObject(ref reader);
                if (result.IsFailure) throw new JsonException(result.Error.Message);
                return result.Value;
            }

            throw new JsonException("Unexpected JSON payload for UserStatus.");
        }

        public override void Write(Utf8JsonWriter writer, UserStatus value, JsonSerializerOptions options) {
            if (value is null) {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStartObject();
            writer.WriteString(nameof(UserStatus.Code), value.Code);
            writer.WriteEndObject();
        }
    }

    public sealed class UserStatusTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            if (value is string strValue) {
                if (TryParse(strValue, culture, out var parsed)) return parsed;
                throw new ArgumentException($"Invalid UserStatus: '{strValue}'. Valid values are 'Disabled', 'Enabled', 'Blocked', 'Deleted', or 'Suspended'.");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
            if (value is UserStatus status && destinationType == typeof(string)) return status.Code;
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion

    public static implicit operator string?(UserStatus? status) => status?.Code;
    public static implicit operator UserStatus?(string? value) {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return Parse(value);
    }

    public override string ToString() => this.Code;

    private static bool LooksLikeJsonObjectText(string value) => value.StartsWith('{') && value.EndsWith('}');

    private static bool TryParseJsonObjectText(string value, out UserStatus? result) {
        result = null;

        try {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;

            foreach (var property in document.RootElement.EnumerateObject()) {
                if (!string.Equals(property.Name, nameof(UserStatus.Code), StringComparison.OrdinalIgnoreCase)) continue;

                var creationResult = From(new UserStatusConstructorArgs { Code = property.Value.GetString() ?? string.Empty });
                if (creationResult.IsFailure) return false;

                result = creationResult.Value;
                return true;
            }

            return false;
        } catch {
            return false;
        }
    }

    private static IResult<UserStatus> ReadFromObject(ref Utf8JsonReader reader) {
        string? code = null;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            if (!reader.Read()) break;

            if (string.Equals(propertyName, nameof(UserStatus.Code), StringComparison.OrdinalIgnoreCase)) {
                code = reader.GetString();
            }
        }

        return From(new UserStatusConstructorArgs { Code = code ?? string.Empty });
    }
}
