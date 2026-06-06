using System.ComponentModel;
using System.Text.Json;
using Sandlada.Extension.Auth.Domain.ValueObjects;

namespace Sandlada.Extension.Auth.Domain.Tests.ValueObjects;

public sealed class EmailAddressTests {
    [Fact]
    public void From_NameAndDomainOverload_ValidInput_ReturnsSuccess() {
        var result = EmailAddress.From(" user.name ", " example.com ");

        Assert.True(result.IsSuccess);
        Assert.Equal("user.name", result.Value.Name);
        Assert.Equal("example.com", result.Value.Domain);
        Assert.Equal("user.name@example.com", result.Value.Value);
    }

    [Theory]
    [InlineData("", "example.com", "Email.InvalidName")]
    [InlineData("user", "", "Email.InvalidDomain")]
    [InlineData("user+tag", "example.com", "Email.InvalidName")]
    [InlineData("user", "example", "Email.InvalidDomain")]
    public void From_NameAndDomainOverload_InvalidInput_ReturnsFailure(string name, string domain, string expectedCode) {
        var result = EmailAddress.From(name, domain);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
    }

    [Theory]
    [InlineData("user@example.com", "user", "example.com")]
    [InlineData(" user.name@example.co.uk ", "user.name", "example.co.uk")]
    public void From_ValidInput_ReturnsSuccess(string input, string expectedName, string expectedDomain) {
        var result = EmailAddress.From(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedName, result.Value.Name);
        Assert.Equal(expectedDomain, result.Value.Domain);
    }

    [Fact]
    public void JsonConverter_Deserialize_Null_ReturnsNull() {
        var result = JsonSerializer.Deserialize<EmailAddress?>("null");

        Assert.Null(result);
    }

    [Theory]
    [InlineData("", "Email.Empty", "Email cannot be empty.")]
    [InlineData("   ", "Email.Empty", "Email cannot be empty.")]
    [InlineData("@@.com", "Email.MissingAtSymbol", "Email must contain exactly one @ symbol.")]
    [InlineData("@example.com", "Email.InvalidName", "Email name part cannot be empty or invalid.")]
    [InlineData("user@", "Email.InvalidDomain", "Email domain part cannot be empty or invalid.")]
    [InlineData("user@example", "Email.InvalidDomain", "Email domain part cannot be empty or invalid.")]
    [InlineData("user@example..com", "Email.InvalidDomain", "Email domain part cannot be empty or invalid.")]
    [InlineData("user@exa mple.com", "Email.InvalidFormat", "Email format is incorrect.")]
    [InlineData("user+tag@example.com", "Email.InvalidName", "Email name part cannot be empty or invalid.")]
    [InlineData(".user@example.com", "Email.InvalidName", "Email name part cannot be empty or invalid.")]
    [InlineData("user.@example.com", "Email.InvalidName", "Email name part cannot be empty or invalid.")]
    [InlineData("user..name@example.com", "Email.InvalidName", "Email name part cannot be empty or invalid.")]
    public void From_InvalidInput_ReturnsExpectedFailure(string input, string expectedCode, string expectedMessage) {
        var result = EmailAddress.From(input);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedCode, result.Error.Code);
        Assert.Equal(expectedMessage, result.Error.Message);
    }

    [Theory]
    [InlineData("\"user@example.com\"", "user", "example.com")]
    [InlineData("\" user.name@example.co.uk \"", "user.name", "example.co.uk")]
    public void JsonConverter_Deserialize_String_ReturnsExpectedEmail(string json, string expectedName, string expectedDomain) {
        var result = JsonSerializer.Deserialize<EmailAddress?>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedName, result!.Name);
        Assert.Equal(expectedDomain, result.Domain);
    }

    [Theory]
    [InlineData("{\"Name\":\"user\",\"Domain\":\"example.com\"}", "user", "example.com")]
    [InlineData("{\"name\":\" user.name \",\"domain\":\" example.co.uk \"}", "user.name", "example.co.uk")]
    public void JsonConverter_Deserialize_Object_ReturnsExpectedEmail(string json, string expectedName, string expectedDomain) {
        var result = JsonSerializer.Deserialize<EmailAddress?>(json);

        Assert.NotNull(result);
        Assert.Equal(expectedName, result!.Name);
        Assert.Equal(expectedDomain, result.Domain);
    }

    [Theory]
    [InlineData("\"\"", "Email cannot be empty.")]
    [InlineData("\"@@.com\"", "Email must contain exactly one @ symbol.")]
    [InlineData("\"user@\"", "Email domain part cannot be empty or invalid.")]
    [InlineData("\".user@example.com\"", "Email name part cannot be empty or invalid.")]
    [InlineData("\"user..name@example.com\"", "Email name part cannot be empty or invalid.")]
    [InlineData("123", "Unexpected JSON payload for EmailAddress.")]
    public void JsonConverter_Deserialize_InvalidString_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EmailAddress?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("{\"Name\":\"user\"}", "Email domain part cannot be empty or invalid.")]
    [InlineData("{\"Domain\":\"example.com\"}", "Email name part cannot be empty or invalid.")]
    public void JsonConverter_Deserialize_Object_MissingProperty_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EmailAddress?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Theory]
    [InlineData("user@example.com", "{\"Name\":\"user\",\"Domain\":\"example.com\"}")]
    [InlineData(" user.name@example.co.uk ", "{\"Name\":\"user.name\",\"Domain\":\"example.co.uk\"}")]
    public void JsonConverter_Serialize_WritesExpectedJsonObject(string input, string expectedJson) {
        var email = EmailAddress.From(input).Value;

        var json = JsonSerializer.Serialize(email);

        Assert.Equal(expectedJson, json);
    }

    [Theory]
    [InlineData("{\"Name\":\"user\",\"Domain\":\"example.com\"}", "user", "example.com")]
    [InlineData(" {\"name\":\" user.name \",\"domain\":\" example.co.uk \"} ", "user.name", "example.co.uk")]
    public void Parse_ObjectText_ReturnsExpectedEmail(string input, string expectedName, string expectedDomain) {
        var result = EmailAddress.Parse(input);

        Assert.Equal(expectedName, result.Name);
        Assert.Equal(expectedDomain, result.Domain);
    }

    [Fact]
    public void TypeConverter_ConvertFromString_ReturnsExpectedEmail() {
        var converter = TypeDescriptor.GetConverter(typeof(EmailAddress));

        var result = converter.ConvertFromInvariantString("user@example.com");

        var email = Assert.IsType<EmailAddress>(result);
        Assert.Equal("user", email.Name);
        Assert.Equal("example.com", email.Domain);
    }

    [Fact]
    public void TypeConverter_ConvertFromString_InvalidInput_ThrowsFormatException() {
        var converter = TypeDescriptor.GetConverter(typeof(EmailAddress));

        Assert.Throws<FormatException>(() => converter.ConvertFromInvariantString("user@exa mple.com"));
    }

    [Fact]
    public void TypeConverter_ConvertToString_ReturnsExpectedEmailString() {
        var converter = TypeDescriptor.GetConverter(typeof(EmailAddress));
        var email = EmailAddress.From("user@example.com").Value;

        var result = converter.ConvertToInvariantString(email);

        Assert.Equal("user@example.com", result);
    }

    [Theory]
    [InlineData("{\"Name\":null,\"Domain\":\"example.com\"}", "Email name part cannot be empty or invalid.")]
    [InlineData("{\"Name\":\"user\",\"Domain\":null}", "Email domain part cannot be empty or invalid.")]
    [InlineData("{\"Name\":\"  \",\"Domain\":\"example.com\"}", "Email name part cannot be empty or invalid.")]
    [InlineData("{\"Name\":\"user\",\"Domain\":\"  \"}", "Email domain part cannot be empty or invalid.")]
    public void JsonConverter_Deserialize_Object_InvalidValues_ThrowsJsonException(string json, string expectedMessage) {
        var exception = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<EmailAddress?>(json));

        Assert.Equal(expectedMessage, exception.Message);
    }

    [Fact]
    public void Parse_NullInput_ThrowsFormatException() {
        Assert.Throws<FormatException>(() => EmailAddress.Parse(null!));
    }

    [Fact]
    public void TryParse_NullInput_ReturnsFalse() {
        var result = EmailAddress.TryParse(null, null, out var email);

        Assert.False(result);
        Assert.Null(email);
    }

    [Fact]
    public void JsonConverter_RoundTrip_ReturnsEquivalentEmail() {
        var email = EmailAddress.From("user.name@example.co.uk").Value;

        var json = JsonSerializer.Serialize(email);
        var roundTrip = JsonSerializer.Deserialize<EmailAddress>(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(email, roundTrip);
    }

    [Fact]
    public void Indexer_ValidIndexes_ReturnNameAndDomain() {
        var email = EmailAddress.From("user@example.com").Value;

        Assert.Equal("user", email[0]);
        Assert.Equal("example.com", email[1]);
    }

    [Fact]
    public void Indexer_InvalidIndex_ThrowsIndexOutOfRangeExceptionWithDomainErrorCode() {
        var email = EmailAddress.From("user@example.com").Value;

        var exception = Assert.Throws<IndexOutOfRangeException>(() => {
            _ = email[2];
        });

        Assert.Contains("Email.InvalidIndex", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"Name\":\"user\"}")]
    [InlineData("{\"Domain\":\"example.com\"}")]
    [InlineData("{\"Name\":123,\"Domain\":\"example.com\"}")]
    public void TryParse_InvalidObjectText_ReturnsFalse(string input) {
        var success = EmailAddress.TryParse(input, null, out var email);

        Assert.False(success);
        Assert.Null(email);
    }
}
