namespace Sandlada.Extension.Auth.Infrastructure.Persistence;

internal static class InfrastructureNormalization {
    public static string Normalize(string value) => value.Trim().ToUpperInvariant();

    public static string? NormalizeNullable(string? value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return null;
        }

        return Normalize(value);
    }
}
