using Otomasyon.Domain.Exceptions;

namespace Otomasyon.Domain.Entities;

public sealed class AuthUser:BaseEntity
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string HashPassword { get; private set; } = null!;
    public bool IsActive { get; set; }
    public ICollection<AuthUserRole> AuthUserRoles { get; set; } = new HashSet<AuthUserRole>();
    // user'ın birden fazla refresh token'ı olacak - 1 den fazla cihazdan girebilir
    public ICollection<AuthRefreshToken> AuthRefreshTokens { get; set; } = new HashSet<AuthRefreshToken>();
    
    public void SetPassword(string password, Func<string, string> hashFunc)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new PasswordLengthException();

        if (password.Length < 4)
            throw new PasswordLengthException();

        HashPassword = hashFunc(password);
    }
}