using Sandlada.Extension.Auth.Domain.Commons;
using Sandlada.Extension.Auth.Domain.Primitives;

namespace Sandlada.Extension.Auth.Domain.Aggregates;

public sealed record OAuthClientConstructorArgs {
    public required Guid Id { get; init; }
    public required string ClientId { get; init; }
    public required string DisplayName { get; init; }
    public required List<string> RedirectUris { get; init; }
    public List<string> PostLogoutRedirectUris { get; init; } = [];
    public List<string> AllowedScopes { get; init; } = [];
    public List<string> AllowedGrantTypes { get; init; } = ["authorization_code", "refresh_token"];
    public required DateTime CreatedAt { get; init; }
    public required DateTime UpdatedAt { get; init; }
}

public sealed class OAuthClient : IAggregate<Guid> {

    #region Properties

    public Guid Id { get; private set; }
    public string ClientId { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public List<string> RedirectUris { get; private set; } = [];
    public List<string> PostLogoutRedirectUris { get; private set; } = [];
    public List<string> AllowedScopes { get; private set; } = [];
    public List<string> AllowedGrantTypes { get; private set; } = [];
    public DateTime CreatedAt { get; private set; } = DateTime.UnixEpoch;
    public DateTime UpdatedAt { get; private set; } = DateTime.UnixEpoch;

    #endregion

    #region Constructors

    private OAuthClient() {
    }

    private OAuthClient(OAuthClientConstructorArgs args) {
        this.Id = args.Id;
        this.ClientId = args.ClientId;
        this.DisplayName = args.DisplayName;
        this.RedirectUris = [..args.RedirectUris];
        this.PostLogoutRedirectUris = [..args.PostLogoutRedirectUris];
        this.AllowedScopes = [..args.AllowedScopes];
        this.AllowedGrantTypes = [..args.AllowedGrantTypes];
        this.CreatedAt = args.CreatedAt;
        this.UpdatedAt = args.UpdatedAt;
    }

    public static IResult<OAuthClient> From(OAuthClientConstructorArgs args) {
        var clientId = args.ClientId.Trim();
        if (string.IsNullOrWhiteSpace(clientId)) {
            return Result.Failure<OAuthClient>(DomainError.OAuthClient.ClientIdCannotBeEmpty);
        }

        if (clientId.Length < 3) {
            return Result.Failure<OAuthClient>(DomainError.OAuthClient.ClientIdTooShort);
        }

        if (clientId.Length > 100) {
            return Result.Failure<OAuthClient>(DomainError.OAuthClient.ClientIdTooLong);
        }

        var displayName = args.DisplayName.Trim();
        if (string.IsNullOrWhiteSpace(displayName)) {
            return Result.Failure<OAuthClient>(DomainError.OAuthClient.DisplayNameCannotBeEmpty);
        }

        if (displayName.Length > 200) {
            return Result.Failure<OAuthClient>(DomainError.OAuthClient.DisplayNameTooLong);
        }

        if (args.RedirectUris.Count == 0) {
            return Result.Failure<OAuthClient>(DomainError.OAuthClient.RedirectUriCannotBeEmpty);
        }

        foreach (var uri in args.RedirectUris) {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out _)) {
                return Result.Failure<OAuthClient>(DomainError.OAuthClient.InvalidRedirectUri);
            }
        }

        foreach (var uri in args.PostLogoutRedirectUris) {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out _)) {
                return Result.Failure<OAuthClient>(DomainError.OAuthClient.InvalidPostLogoutRedirectUri);
            }
        }

        return Result.Success(new OAuthClient(args with {
            ClientId = clientId,
            DisplayName = displayName,
        }));
    }

    #endregion

    #region Methods: Update Properties

    public IResult UpdateDisplayName(string displayName) {
        var normalized = displayName.Trim();
        if (string.IsNullOrWhiteSpace(normalized)) {
            return Result.Failure(DomainError.OAuthClient.DisplayNameCannotBeEmpty);
        }
        if (normalized.Length > 200) {
            return Result.Failure(DomainError.OAuthClient.DisplayNameTooLong);
        }
        this.DisplayName = normalized;
        this.UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public IResult UpdateRedirectUris(List<string> redirectUris) {
        if (redirectUris.Count == 0) {
            return Result.Failure(DomainError.OAuthClient.RedirectUriCannotBeEmpty);
        }
        foreach (var uri in redirectUris) {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out _)) {
                return Result.Failure(DomainError.OAuthClient.InvalidRedirectUri);
            }
        }
        this.RedirectUris = [..redirectUris];
        this.UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public IResult UpdatePostLogoutRedirectUris(List<string> postLogoutRedirectUris) {
        foreach (var uri in postLogoutRedirectUris) {
            if (!Uri.TryCreate(uri, UriKind.Absolute, out _)) {
                return Result.Failure(DomainError.OAuthClient.InvalidPostLogoutRedirectUri);
            }
        }
        this.PostLogoutRedirectUris = [..postLogoutRedirectUris];
        this.UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public IResult UpdateAllowedScopes(List<string> allowedScopes) {
        this.AllowedScopes = [..allowedScopes];
        this.UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public IResult UpdateAllowedGrantTypes(List<string> allowedGrantTypes) {
        this.AllowedGrantTypes = [..allowedGrantTypes];
        this.UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    #endregion
}