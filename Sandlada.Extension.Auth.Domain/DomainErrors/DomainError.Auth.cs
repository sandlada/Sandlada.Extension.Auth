namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class Auth {
        public static readonly DomainError InvalidCredentials = new("Auth.InvalidCredentials", "The email, unique name, or password is incorrect.");
        public static readonly DomainError EmailAddressNotVerified = new("Auth.EmailAddressNotVerified", "The email address has not been verified.");
        public static readonly DomainError PasswordCannotBeEmpty = new("Auth.PasswordCannotBeEmpty", "Password cannot be empty.");
        public static readonly DomainError RegistrationRequestLimitExceeded = new("Auth.RegistrationRequestLimitExceeded", "The same email address can only submit 10 registration requests per UTC day.");
        public static readonly DomainError RegistrationProfileAlreadyCompleted = new("Auth.RegistrationProfileAlreadyCompleted", "The registration profile has already been completed.");
        public static readonly DomainError InvalidVerificationChallenge = new("Auth.InvalidVerificationChallenge", "The verification challenge is invalid.");
        public static readonly DomainError VerificationCodeNotFound = new("Auth.VerificationCodeNotFound", "The verification code challenge was not found.");
        public static readonly DomainError InvalidVerificationCode = new("Auth.InvalidVerificationCode", "The verification code is invalid.");
        public static readonly DomainError VerificationCodeAttemptLimitExceeded = new("Auth.VerificationCodeAttemptLimitExceeded", "The verification code challenge has exceeded the allowed attempt limit. Please request a new code.");
        public static readonly DomainError VerificationCodeExpired = new("Auth.VerificationCodeExpired", "The verification code has expired.");
        public static readonly DomainError VerificationCodeAlreadyUsed = new("Auth.VerificationCodeAlreadyUsed", "The verification code has already been used.");
        public static readonly DomainError EmailAddressUnchanged = new("Auth.EmailAddressUnchanged", "The new email address must be different from the current email address.");
        public static readonly DomainError EmailRebindRequestLimitExceeded = new("Auth.EmailRebindRequestLimitExceeded", "The same user can only submit 10 email rebind requests per UTC day.");
        public static readonly DomainError EmailRebindVerificationNotFound = new("Auth.EmailRebindVerificationNotFound", "The email rebind verification challenge was not found.");
        public static readonly DomainError SessionNotFound = new("Auth.SessionNotFound", "The session was not found.");
    }
}
