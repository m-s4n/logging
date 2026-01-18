using Otomasyon.Domain.Exceptions;

namespace Otomasyon.Domain.Entities;

public sealed class AuthRefreshToken : BaseEntity
{
    public Guid Id { get; set; }
    public Guid AuthUserId { get; set; }

    public string TokenHash { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? RevokedAt { get; set; }
    public string? ReplacedByTokenHash { get; set; }

    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }

    public DateTimeOffset? LastUsedAt { get; set; }

    public bool IsActive => RevokedAt is null && ExpiresAt > DateTimeOffset.UtcNow;

    public void SetToken(string rawToken, Func<string, string> hashFunc)
    {
        if (string.IsNullOrWhiteSpace(rawToken))
            throw new PasswordLengthException();

        TokenHash = hashFunc(rawToken);
    }

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}