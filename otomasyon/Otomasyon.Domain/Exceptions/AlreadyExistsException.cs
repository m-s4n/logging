namespace Otomasyon.Domain.Exceptions;

public class AlreadyExistsException(string fieldName, string? value = null)
    :BusinessException(value is null
        ? $"{fieldName} zaten mevcut."
        : $"{fieldName}='{value}' zaten mevcut.",409, "ALREADY_EXISTS");