using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Otomasyon.Shared.Security;

namespace Otomasyon.Service.Services.Security;

public sealed class RefreshTokenHasher : IRefreshTokenHasher
{
    private readonly JwtOptions _opt;
    public RefreshTokenHasher(IOptions<JwtOptions> opt) => _opt = opt.Value;

    public string Hash(string rawRefreshToken)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_opt.SigningKey));
        var bytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawRefreshToken));
        return Convert.ToHexString(bytes);
    }
}
