using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.OAuthClient;

public sealed record InsertOneOAuthClientCommand : IRequest<IResult<InsertOneOAuthClientCommandResponse>> {
    public required string ClientId { get; init; }
    public required string DisplayName { get; init; }
    public required List<string> RedirectUris { get; init; }
    public List<string> PostLogoutRedirectUris { get; init; } = [];
    public List<string> AllowedScopes { get; init; } = [];
    public List<string> AllowedGrantTypes { get; init; } = [];

    [SetsRequiredMembers]
    public InsertOneOAuthClientCommand(InsertOneOAuthClientCommandArgs args) {
        this.ClientId = args.ClientId;
        this.DisplayName = args.DisplayName;
        this.RedirectUris = args.RedirectUris;
        this.PostLogoutRedirectUris = args.PostLogoutRedirectUris;
        this.AllowedScopes = args.AllowedScopes;
        this.AllowedGrantTypes = args.AllowedGrantTypes;
    }
}