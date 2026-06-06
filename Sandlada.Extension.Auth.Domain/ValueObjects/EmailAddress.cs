using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Mail;
using System.Text.Json;
using System.Text.Json.Serialization;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.ValueObjects;

[JsonConverter(typeof(EmailAddressJsonConverter))]
[TypeConverter(typeof(EmailAddressTypeConverter))]
public sealed record EmailAddress : IEquatable<EmailAddress>, IParsable<EmailAddress> {
    #region Properties
    public string Value => $"{this.Name}@{this.Domain}";
    public string Name { get; init; }
    public string Domain { get; init; }
    #endregion

    #region Constructors
    private EmailAddress(string name, string domain) {
        this.Name = name;
        this.Domain = domain;
    }

    public static IResult<EmailAddress> From(string value) {
        if (string.IsNullOrWhiteSpace(value)) return Result.Failure<EmailAddress>(DomainError.Email.Empty);

        var trimmed = value.Trim();
        if (trimmed.Count(c => c == '@') != 1) return Result.Failure<EmailAddress>(DomainError.Email.MissingAtSymbol);

        var parts = trimmed.Split('@', 2);
        var namePart = parts[0].Trim();
        var domainPart = parts[1].Trim();

        if (string.IsNullOrEmpty(namePart)) return Result.Failure<EmailAddress>(DomainError.Email.InvalidName);
        if (string.IsNullOrEmpty(domainPart)) return Result.Failure<EmailAddress>(DomainError.Email.InvalidDomain);
        if (!IsValidName(namePart)) return Result.Failure<EmailAddress>(DomainError.Email.InvalidName);
        if (!IsValidDomain(domainPart)) return Result.Failure<EmailAddress>(DomainError.Email.InvalidDomain);

        try {
            var mailAddress = new MailAddress(trimmed);
            if (mailAddress.Address != trimmed) return Result.Failure<EmailAddress>(DomainError.Email.InvalidFormat);
        } catch {
            return Result.Failure<EmailAddress>(DomainError.Email.InvalidFormat);
        }

        return Result.Success(new EmailAddress(namePart, domainPart));
    }

    public static IResult<EmailAddress> From(string name, string domain) {
        if (string.IsNullOrWhiteSpace(name)) return Result.Failure<EmailAddress>(DomainError.Email.InvalidName);
        if (string.IsNullOrWhiteSpace(domain)) return Result.Failure<EmailAddress>(DomainError.Email.InvalidDomain);

        return From($"{name.Trim()}@{domain.Trim()}");
    }
    #endregion

    #region Methods: Validation

    private static bool IsValidName(string name) {
        if (string.IsNullOrWhiteSpace(name)) return false;
        if (name.StartsWith('.') || name.EndsWith('.')) return false;
        if (name.Contains("..")) return false;

        var hasAlphanumeric = false;
        foreach (var c in name) {
            if (char.IsLetterOrDigit(c)) hasAlphanumeric = true;
            else if (c != '.' && c != '_' && c != '-') return false;
        }

        return hasAlphanumeric;
    }

    private static bool IsValidDomain(string domain) {
        if (string.IsNullOrWhiteSpace(domain)) return false;
        if (!domain.Contains('.')) return false;
        if (domain.StartsWith('.') || domain.EndsWith('.')) return false;
        if (domain.Contains("..")) return false;

        foreach (var label in domain.Split('.')) {
            if (string.IsNullOrEmpty(label) || !label.Any(char.IsLetterOrDigit)) return false;
        }

        return true;
    }

    #endregion

    #region Methods: IParsable

    public static EmailAddress Parse(string s, IFormatProvider? provider = null) {
        if (!TryParse(s, provider, out var result)) throw new FormatException($"Invalid email format: {s}");
        return result;
    }

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out EmailAddress result) {
        result = default!;
        if (s is null) return false;

        var trimmed = s.Trim();
        if (LooksLikeJsonObjectText(trimmed)) {
            return TryParseJsonObjectText(trimmed, out result);
        }

        var creationResult = From(trimmed);
        if (creationResult.IsFailure) return false;

        result = creationResult.Value;
        return true;
    }

    #endregion

    #region Methods: JSON

    public sealed class EmailAddressJsonConverter : JsonConverter<EmailAddress> {
        public override EmailAddress? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
            if (reader.TokenType == JsonTokenType.Null) return null;

            if (reader.TokenType == JsonTokenType.String) {
                var value = reader.GetString();
                if (value is null) return null;

                var result = From(value);
                if (result.IsFailure) throw new JsonException(result.Error.Message);
                return result.Value;
            }

            if (reader.TokenType == JsonTokenType.StartObject) {
                var result = ReadFromObject(ref reader);
                if (result.IsFailure) throw new JsonException(result.Error.Message);
                return result.Value;
            }

            throw new JsonException("Unexpected JSON payload for EmailAddress.");
        }

        public override void Write(Utf8JsonWriter writer, EmailAddress value, JsonSerializerOptions options) {
            writer.WriteStartObject();
            writer.WriteString(nameof(EmailAddress.Name), value.Name);
            writer.WriteString(nameof(EmailAddress.Domain), value.Domain);
            writer.WriteEndObject();
        }
    }

    public sealed class EmailAddressTypeConverter : TypeConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value) {
            if (value is string s) {
                if (TryParse(s, culture, out var parsed)) return parsed;
                throw new FormatException($"Invalid email format: {s}");
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType) => destinationType == typeof(string) || base.CanConvertTo(context, destinationType);

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType) {
            if (value is EmailAddress emailAddress && destinationType == typeof(string)) return emailAddress.Value;
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    #endregion

    public static implicit operator string(EmailAddress email) => email.Value;
    public override string ToString() => this.Value;

    public string this[int index] => index switch {
        0 => this.Name,
        1 => this.Domain,
        _ => throw new IndexOutOfRangeException($"${DomainError.Email.InvalidIndex.Code}: {DomainError.Email.InvalidIndex.Message}")
    };

    private static bool LooksLikeJsonObjectText(string value) => value.StartsWith('{') && value.EndsWith('}');

    private static bool TryParseJsonObjectText(string value, out EmailAddress result) {
        result = default!;

        try {
            using var document = JsonDocument.Parse(value);
            if (document.RootElement.ValueKind != JsonValueKind.Object) return false;

            string? name = null;
            string? domain = null;

            foreach (var property in document.RootElement.EnumerateObject()) {
                if (string.Equals(property.Name, nameof(EmailAddress.Name), StringComparison.OrdinalIgnoreCase)) {
                    name = property.Value.GetString();
                } else if (string.Equals(property.Name, nameof(EmailAddress.Domain), StringComparison.OrdinalIgnoreCase)) {
                    domain = property.Value.GetString();
                }
            }

            var creationResult = From(name ?? string.Empty, domain ?? string.Empty);
            if (creationResult.IsFailure) return false;

            result = creationResult.Value;
            return true;
        } catch {
            return false;
        }
    }

    private static IResult<EmailAddress> ReadFromObject(ref Utf8JsonReader reader) {
        string? name = null;
        string? domain = null;

        while (reader.Read()) {
            if (reader.TokenType == JsonTokenType.EndObject) break;
            if (reader.TokenType != JsonTokenType.PropertyName) continue;

            var propertyName = reader.GetString();
            if (!reader.Read()) break;

            if (string.Equals(propertyName, nameof(EmailAddress.Name), StringComparison.OrdinalIgnoreCase)) {
                name = reader.GetString();
            } else if (string.Equals(propertyName, nameof(EmailAddress.Domain), StringComparison.OrdinalIgnoreCase)) {
                domain = reader.GetString();
            }
        }

        return From(name ?? string.Empty, domain ?? string.Empty);
    }
}
