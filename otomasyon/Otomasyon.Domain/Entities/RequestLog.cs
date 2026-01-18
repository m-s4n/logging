namespace Otomasyon.Domain.Entities;

public class RequestLog
{
    public long Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    
    public string Method { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string? QueryString { get; set; }
    
    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    
    public long Duration { get; set; }
    public string Level { get; set; } = null!;
    public string TraceId { get; set; } = null!;
    
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; } = null!;
    
    public bool IsDispatched { get; set; }          // default false
    public DateTimeOffset? DispatchedAt { get; set; }

}