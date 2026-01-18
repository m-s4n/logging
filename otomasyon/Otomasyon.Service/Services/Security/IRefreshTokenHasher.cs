
namespace Otomasyon.Service.Services.Security;

public interface IRefreshTokenHasher
{
    string Hash(string rawRefreshToken);
}
