namespace Otomasyon.Domain.Exceptions;

public abstract class BusinessException(string message, int statusCode, string errorCode) : Exception(message)
{
    public int StatusCode { get;} = statusCode;
    public string ErrorCode { get; } = errorCode;
}