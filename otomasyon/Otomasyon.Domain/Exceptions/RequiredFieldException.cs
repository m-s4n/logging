namespace Otomasyon.Domain.Exceptions;

public class RequiredFieldException(string fieldName) 
    :BusinessException($"'{fieldName}' alanı zorunludur.", 400,"VALIDATION_REQUIRED_FIELD");