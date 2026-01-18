namespace Otomasyon.Domain.Exceptions;

public class EntityNotFoundException<T>(object entityId) :
    BusinessException(
        message: $"{typeof(T).Name} bulunamadı. (Id: {entityId})",
        statusCode: 404,
        errorCode: $"{typeof(T).Name}_NOT_FOUND");