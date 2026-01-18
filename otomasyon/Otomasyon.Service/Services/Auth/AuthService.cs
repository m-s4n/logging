using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Otomasyon.Shared.Security;
using Otomasyon.DataAccess.Contexts;
using Otomasyon.Domain.Entities;
using Otomasyon.Domain.Exceptions;
using Otomasyon.Service.Services.Auth;
using Otomasyon.Service.Services.PasswordHasher;
using Otomasyon.Service.Services.Security;


namespace Otomasyon.Services.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IRefreshTokenHasher _refreshTokenHasher;
    private readonly JwtOptions _opt;

    public AuthService(
        AppDbContext db,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        IRefreshTokenHasher refreshTokenHasher,
        IOptions<JwtOptions> opt)
    {
        _db = db;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _refreshTokenHasher = refreshTokenHasher;
        _opt = opt.Value;
    }

    public async Task<(string accessToken, string refreshToken)> LoginAsync(string email, string password, string ip, string? ua)
    {
        var user = await _db.AuthUsers
            .Include(x => x.AuthUserRoles)
                .ThenInclude(ur => ur.AuthRole)
            .FirstOrDefaultAsync(x => x.Email == email);

        if (user is null || user.IsActive == false)
            throw new UnauthorizedException();

        if (!_passwordHasher.Verify(password, user.HashPassword))
            throw new UnauthorizedException();

        var roles = user.AuthUserRoles
            .Select(ur => ur.AuthRole.Name)
            .ToList();

        var access = _tokenService.CreateAccessToken(user, roles);

        var rawRefresh = _tokenService.CreateRefreshTokenRaw();

        var refreshEntity = new AuthRefreshToken
        {
            AuthUserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_opt.RefreshTokenDays),
            CreatedByIp = ip,
            UserAgent = ua
        };
        refreshEntity.SetToken(rawRefresh, _refreshTokenHasher.Hash);

        _db.AuthRefreshTokens.Add(refreshEntity);
        await _db.SaveChangesAsync();

        return (access, rawRefresh);
    }

    public async Task<(string accessToken, string refreshToken)> RefreshAsync(string refreshTokenRaw, string ip, string? ua)
    {
        var incomingHash = _refreshTokenHasher.Hash(refreshTokenRaw);

        var token = await _db.AuthRefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == incomingHash);

        if (token is null)
            throw new UnauthorizedException();

        // reuse detection
        if (token.RevokedAt is not null)
        {
            await RevokeAllTokensAsync(token.AuthUserId);
            await _db.SaveChangesAsync();
            throw new UnauthorizedException();
        }

        if (token.ExpiresAt <= DateTimeOffset.UtcNow)
            throw new UnauthorizedException();

        var user = await _db.AuthUsers
            .Include(x => x.AuthUserRoles)
                .ThenInclude(ur => ur.AuthRole)
            .FirstAsync(x => x.Id == token.AuthUserId);

        if (user.IsActive == false)
            throw new UnauthorizedException();

        var roles = user.AuthUserRoles
            .Select(ur => ur.AuthRole.Name)
            .ToList();

        // ROTATE
        var newRaw = _tokenService.CreateRefreshTokenRaw();
        var newHash = _refreshTokenHasher.Hash(newRaw);

        token.Revoke(replacedByTokenHash: newHash);
        token.LastUsedAt = DateTimeOffset.UtcNow;

        var newEntity = new AuthRefreshToken
        {
            AuthUserId = user.Id,
            CreatedAt = DateTimeOffset.UtcNow,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_opt.RefreshTokenDays),
            CreatedByIp = ip,
            UserAgent = ua
        };
        newEntity.SetToken(newRaw, _refreshTokenHasher.Hash);

        _db.AuthRefreshTokens.Add(newEntity);

        var access = _tokenService.CreateAccessToken(user, roles);

        await _db.SaveChangesAsync();
        return (access, newRaw);
    }

    public async Task LogoutAsync(string refreshTokenRaw)
    {
        var hash = _refreshTokenHasher.Hash(refreshTokenRaw);

        var token = await _db.AuthRefreshTokens
            .AsTracking()
            .FirstOrDefaultAsync(x => x.TokenHash == hash);

        if (token is null) return;

        if (token.RevokedAt is null)
        {
            token.Revoke();
            await _db.SaveChangesAsync();
        }
    }

    public async Task LogoutAllAsync(Guid userId)
    {
        await RevokeAllTokensAsync(userId);
        await _db.SaveChangesAsync();
    }

    private async Task RevokeAllTokensAsync(Guid userId)
    {
        var now = DateTimeOffset.UtcNow;

        var tokens = await _db.AuthRefreshTokens
            .Where(x => x.AuthUserId == userId && x.RevokedAt == null && x.ExpiresAt > now)
            .ToListAsync();

        foreach (var t in tokens)
            t.Revoke();
    }
}

