
namespace Otomasyon.Service.Services.Security;

public interface ITokenService
{
    string CreateAccessToken(Domain.Entities.AuthUser user, IEnumerable<string> roles);
    string CreateRefreshTokenRaw();
}