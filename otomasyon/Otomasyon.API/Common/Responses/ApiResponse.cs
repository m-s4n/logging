namespace Otomasyon.API.Common.Responses;

public record ApiResponse<T>(
    bool IsSuccess,
    int StatusCode,
    string? ErrorCode,
    string Message,
    string? TraceId,
    T? Data
);