namespace Otomasyon.Domain.Entities;

public class ErrorLog
{
    public long Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

    public string Method { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string? QueryString { get; set; }

    public int StatusCode { get; set; }
    public string? ErrorCode { get; set; }         
    public string? ErrorMessage { get; set; }     

    public string TraceId { get; set; } = null!;
    public Guid? UserId { get; set; }
    public string? IpAddress { get; set; }

    public string ExceptionType { get; set; } = null!;
    public string StackTrace { get; set; } = null!;
    public string? InnerException { get; set; }
    
    public bool IsDispatched { get; set; }          // default false
    public DateTimeOffset? DispatchedAt { get; set; }

}
