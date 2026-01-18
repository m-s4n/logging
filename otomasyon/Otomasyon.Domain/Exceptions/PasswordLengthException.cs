namespace Otomasyon.Domain.Exceptions;

public class PasswordLengthException() :BusinessException
    ("Parola uzunluğu en az 4 karakter olmalıdır", 400,"PASSWORD_TOO_SHORT")
{
    
}