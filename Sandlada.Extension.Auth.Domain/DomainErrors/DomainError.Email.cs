namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class Email {
        public static readonly DomainError Empty = new("Email.Empty", "Email cannot be empty.");
        public static readonly DomainError MissingAtSymbol = new("Email.MissingAtSymbol", "Email must contain exactly one @ symbol.");
        public static readonly DomainError InvalidName = new("Email.InvalidName", "Email name part cannot be empty or invalid.");
        public static readonly DomainError InvalidDomain = new("Email.InvalidDomain", "Email domain part cannot be empty or invalid.");
        public static readonly DomainError InvalidFormat = new("Email.InvalidFormat", "Email format is incorrect.");
        public static readonly DomainError InvalidIndex = new("Email.InvalidIndex", "Email index can only be 0 (Name) or 1 (Domain).");
    }
}
