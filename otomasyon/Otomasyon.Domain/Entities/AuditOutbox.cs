

namespace Otomasyon.Domain.Entities;

public class AuditOutbox
{
    public long Id { get; set; }

    public Guid? UserId { get; set; }

    public string Action { get; set; } = default!;
    public string EntityName { get; set; } = default!;
    public string EntityId { get; set; } = default!;

    // Postgres'te jsonb map'lenecek (diff: sadece değişen alanlar)
    public string ChangesJson { get; set; } = "{}";

    public string? TraceId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public string? QueryString { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public bool IsDispatched { get; set; }
    public DateTimeOffset? DispatchedAt { get; set; }
}
