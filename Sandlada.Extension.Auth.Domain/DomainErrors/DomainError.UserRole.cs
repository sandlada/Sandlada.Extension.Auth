namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class UserRole {
        public static DomainError InvalidValue(string value) => new("UserRole.InvalidValue", $"Invalid UserRole: '{value}'. Valid values are 'Administrator' or 'Normal'.");
    }
}
