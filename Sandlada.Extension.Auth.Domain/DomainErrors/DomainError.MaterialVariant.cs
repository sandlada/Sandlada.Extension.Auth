namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class MaterialVariant {
        public static readonly DomainError Invalid = new("MaterialVariant.Invalid", "Material variant is invalid.");
        public static DomainError InvalidCode(byte code) => new("MaterialVariant.InvalidCode", $"Invalid material variant code: '{code}'. Valid values are 0 to 8.");
        public static DomainError InvalidName(string name) => new("MaterialVariant.InvalidName", $"Invalid material variant name: '{name}'. Valid values are 'Monochrome', 'Neutral', 'TonalSpot', 'Vibrant', 'Expressive', 'Fidelity', 'Content', 'Rainbow', and 'FruitSalad'.");
    }
}
