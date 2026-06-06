namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class MaterialContrastLevel {
        public static readonly DomainError Invalid = new("MaterialContrastLevel.Invalid", "Contrast level is invalid.");
        public static DomainError InvalidLevel(int level) => new("MaterialContrastLevel.InvalidLevel", $"Invalid contrast level: '{level}'. Valid values are -1, 0, or 1.");
    }
}
