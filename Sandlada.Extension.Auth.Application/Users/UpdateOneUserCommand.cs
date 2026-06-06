using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed record UpdateOneUserCommand : IRequest<IResult<UserResponse>> {
    public required Guid UserId { get; init; }
    public string? EmailAddress { get; init; }
    public string? UniqueName { get; init; }
    public string? Role { get; init; }
    public string? Password { get; init; }
    public bool? IsEmailVerified { get; init; }
    public string? Status { get; init; }

    [SetsRequiredMembers]
    public UpdateOneUserCommand(Guid userId, UpdateOneUserCommandArgs args) {
        this.UserId = userId;
        this.EmailAddress = args.EmailAddress;
        this.UniqueName = args.UniqueName;
        this.Role = args.Role;
        this.Password = args.Password;
        this.IsEmailVerified = args.IsEmailVerified;
        this.Status = args.Status;
    }
}
