namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class Gender {
        public static readonly DomainError Empty = new("Gender.Empty", "Gender cannot be empty.");
        public static readonly DomainError ValueTooShort = new("Gender.ValueTooShort", "Gender value is too short. Minimum length is 2.");
        public static readonly DomainError ValueTooLong = new("Gender.ValueTooLong", "Gender value is too long. Maximum length is 16.");
    }
}
