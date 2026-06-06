namespace Sandlada.Extension.Auth.Infrastructure.Persistence.Entities;

public sealed class AuthSessionEntity {
    public string SessionId { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public byte[] TicketData { get; set; } = Array.Empty<byte>();
    public DateTimeOffset? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
