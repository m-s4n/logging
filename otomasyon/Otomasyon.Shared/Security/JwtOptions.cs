
namespace Otomasyon.Shared.Security;

public sealed class JwtOptions
{
    public string Issuer { get; set; } = null!;
    public string Audience { get; set; } = null!;
    public string SigningKey { get; set; } = null!;
    public int AccessTokenMinutes { get; set; }
    public int RefreshTokenDays { get; set; }
}
