
namespace Otomasyon.API.Common.Responses;

public static class ApiResponses
{
    public static ApiResponse<T> Success<T>(T? data, string message = "Success", int statusCode = 200)
        => new(true, statusCode, null, message, null, data);

    public static ApiResponse<T> Fail<T>(string message, string? errorCode, int statusCode, string traceId)
        => new(false, statusCode, errorCode, message, traceId, default);
}

