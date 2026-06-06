namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class User {
        public static readonly DomainError NotFound = new("User.NotFound", "User with the specified ID was not found.");
        public static readonly DomainError UpsertKeyRequired = new("User.UpsertKeyRequired", "Either UserId or EmailAddress is required for insert-or-update operations.");
        public static readonly DomainError EmailAddressAlreadyExists = new("User.EmailAddressAlreadyExists", "A user with the same Email address already exists.");
        public static readonly DomainError UniqueNameAlreadyExists = new("User.UniqueNameAlreadyExists", "A user with the same Unique name already exists.");
        public static readonly DomainError UniqueNameTooShort = new("User.UniqueNameTooShort", "Unique name must be at least 3 characters long.");
        public static readonly DomainError UniqueNameTooLong = new("User.UniqueNameTooLong", "Unique name must be at most 50 characters long.");
        public static readonly DomainError InvalidStatus = new("User.InvalidStatusValue", "The user status value is invalid.");
    }
}
