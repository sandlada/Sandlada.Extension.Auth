using Sandlada.Extension.Auth.Domain.Commons;
using MediatR;
using System.Diagnostics.CodeAnalysis;

namespace Sandlada.Extension.Auth.Application.Users;

public sealed record InsertOneUserCommand : IRequest<IResult<UserResponse>> {
    public required string EmailAddress { get; init; }
    public required string? UniqueName { get; init; }
    public required string? Role { get; init; }
    public required string Password { get; init; }
    public required bool IsEmailVerified { get; init; }
    public string? Status { get; init; }

    [SetsRequiredMembers]
    public InsertOneUserCommand(InsertOneUserCommandArgs args) {
        this.EmailAddress = args.EmailAddress;
        this.UniqueName = args.UniqueName;
        this.Role = args.Role;
        this.Password = args.Password;
        this.IsEmailVerified = args.IsEmailVerified;
        this.Status = args.Status;
    }
}
