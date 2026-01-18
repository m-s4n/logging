

namespace Otomasyon.Service.Services.Auth;

public interface IAuthService
{
    Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password, string ip, string? ua);
    Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshTokenRaw, string ip, string? ua);
    Task LogoutAsync(string refreshTokenRaw);
    Task LogoutAllAsync(Guid userId);
}
