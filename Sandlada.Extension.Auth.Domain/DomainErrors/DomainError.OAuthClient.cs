namespace Sandlada.Extension.Auth.Domain.Commons;

public sealed partial record DomainError {
    public static partial class OAuthClient {
        public static readonly DomainError NotFound = new("OAuthClient.NotFound", "OAuth client with the specified ID was not found.");
        public static readonly DomainError ClientIdAlreadyExists = new("OAuthClient.ClientIdAlreadyExists", "An OAuth client with the same ClientId already exists.");
        public static readonly DomainError ClientIdCannotBeEmpty = new("OAuthClient.ClientIdCannotBeEmpty", "ClientId cannot be empty.");
        public static readonly DomainError ClientIdTooShort = new("OAuthClient.ClientIdTooShort", "ClientId must be at least 3 characters long.");
        public static readonly DomainError ClientIdTooLong = new("OAuthClient.ClientIdTooLong", "ClientId must be at most 100 characters long.");
        public static readonly DomainError DisplayNameCannotBeEmpty = new("OAuthClient.DisplayNameCannotBeEmpty", "DisplayName cannot be empty.");
        public static readonly DomainError DisplayNameTooLong = new("OAuthClient.DisplayNameTooLong", "DisplayName must be at most 200 characters long.");
        public static readonly DomainError RedirectUriCannotBeEmpty = new("OAuthClient.RedirectUriCannotBeEmpty", "At least one RedirectUri is required.");
        public static readonly DomainError InvalidRedirectUri = new("OAuthClient.InvalidRedirectUri", "RedirectUri must be a valid absolute URI.");
        public static readonly DomainError InvalidPostLogoutRedirectUri = new("OAuthClient.InvalidPostLogoutRedirectUri", "PostLogoutRedirectUri must be a valid absolute URI.");
    }
}