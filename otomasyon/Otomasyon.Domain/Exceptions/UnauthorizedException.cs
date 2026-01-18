namespace Otomasyon.Domain.Exceptions;

public class UnauthorizedException() 
    : BusinessException("Unauthorized", 401, "AUTH_UNAUTHORIZED");