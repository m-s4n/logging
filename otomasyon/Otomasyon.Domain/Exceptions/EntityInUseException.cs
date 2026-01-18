namespace Otomasyon.Domain.Exceptions;

public class EntityInUseException<T>(Guid entityId) 
    :BusinessException($"{typeof(T).Name} kullanımda olduğu için silinemez. Id: {entityId}",409, "ENTITY_IN_USE");