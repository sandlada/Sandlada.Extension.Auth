namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class UserStatus {
        public static readonly DomainError InvalidStatus = new("User.InvalidStatus", "The user status is invalid.");
    }
}
