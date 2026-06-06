using System.Reflection;
using Sandlada.Extension.Auth.Domain.Commons;

namespace Sandlada.Extension.Auth.Domain.Tests.Commons;

public sealed class DomainErrorTests {
    [Fact]
    public void GeneralNone_HasEmptyCodeAndMessage() {
        Assert.Equal(string.Empty, DomainError.General.None.Code);
        Assert.Equal(string.Empty, DomainError.General.None.Message);
    }

    [Fact]
    public void RecordEquality_SameCodeAndMessage_AreEqual() {
        var first = new DomainError("Code.Sample", "Message sample");
        var second = new DomainError("Code.Sample", "Message sample");

        Assert.Equal(first, second);
    }

    [Fact]
    public void DynamicFactory_UserRoleInvalidValue_ContainsInputInMessage() {
        var error = DomainError.UserRole.InvalidValue("guest");

        Assert.Equal("UserRole.InvalidValue", error.Code);
        Assert.Contains("guest", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DynamicFactory_MaterialVariantInvalidCode_ContainsCodeValue() {
        var error = DomainError.MaterialVariant.InvalidCode(9);

        Assert.Equal("MaterialVariant.InvalidCode", error.Code);
        Assert.Contains("9", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DynamicFactory_MaterialVariantInvalidName_ContainsNameValue() {
        var error = DomainError.MaterialVariant.InvalidName("mystery");

        Assert.Equal("MaterialVariant.InvalidName", error.Code);
        Assert.Contains("mystery", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void DynamicFactory_MaterialContrastLevelInvalidLevel_ContainsLevelValue() {
        var error = DomainError.MaterialContrastLevel.InvalidLevel(2);

        Assert.Equal("MaterialContrastLevel.InvalidLevel", error.Code);
        Assert.Contains("2", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void StaticErrors_ExceptGeneralNone_HaveNonEmptyCodeAndMessage() {
        var staticErrors = GetStaticDomainErrors();

        foreach (var error in staticErrors) {
            if (error == DomainError.General.None) continue;

            Assert.False(string.IsNullOrWhiteSpace(error.Code));
            Assert.False(string.IsNullOrWhiteSpace(error.Message));
        }
    }

    [Fact]
    public void StaticErrors_ExceptGeneralNone_HaveUniqueCodes() {
        var duplicateCodes = GetStaticDomainErrors()
            .Where(error => error != DomainError.General.None)
            .GroupBy(error => error.Code, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        Assert.Empty(duplicateCodes);
    }

    private static IReadOnlyCollection<DomainError> GetStaticDomainErrors() {
        var nestedTypes = typeof(DomainError).GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic);
        var fields = nestedTypes
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(field => field.FieldType == typeof(DomainError));

        var errors = new List<DomainError>();
        foreach (var field in fields) {
            if (field.GetValue(null) is DomainError error) {
                errors.Add(error);
            }
        }

        return errors;
    }
}
