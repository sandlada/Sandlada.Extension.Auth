namespace Sandlada.Extension.Auth.Application.Auth;

public sealed record RemoveManyUserSessionsByUserIdCommandResponse {
    public required int RemovedCount { get; init; }
}
