namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class UserProfile {
        public static readonly DomainError Invalid = new("UserProfile.Invalid", "User profile is invalid.");
        public static readonly DomainError NotFound = new("UserProfile.NotFound", "User profile was not found.");
        public static readonly DomainError AlreadyExists = new("UserProfile.AlreadyExists", "User profile already exists for the user.");
        public static readonly DomainError UserIdCannotBeEmpty = new("UserProfile.UserIdCannotBeEmpty", "User profile user ID cannot be empty.");
        public static readonly DomainError CreatedAtInvalid = new("UserProfile.CreatedAtInvalid", "User profile creation time is invalid.");
        public static readonly DomainError UpdatedAtInvalid = new("UserProfile.UpdatedAtInvalid", "User profile update time is invalid.");
        public static readonly DomainError UpdatedAtEarlierThanCreatedAt = new("UserProfile.UpdatedAtEarlierThanCreatedAt", "User profile update time cannot be earlier than creation time.");
        public static readonly DomainError GeneralInvalid = new("UserProfile.GeneralInvalid", "General profile settings are invalid.");
        public static readonly DomainError PersonalizationInvalid = new("UserProfile.PersonalizationInvalid", "Personalization settings are invalid.");
        public static readonly DomainError AppearanceInvalid = new("UserProfile.AppearanceInvalid", "Appearance settings are invalid.");
    }
}
