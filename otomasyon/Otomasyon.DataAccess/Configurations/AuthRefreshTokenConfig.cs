using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Otomasyon.Domain.Entities;

namespace Otomasyon.DataAccess.Configurations;

public sealed class AuthRefreshTokenConfiguration : IEntityTypeConfiguration<AuthRefreshToken>
{
    public void Configure(EntityTypeBuilder<AuthRefreshToken> b)
    {
        b.ToTable("auth_refresh_tokens");

        b.HasKey(x => x.Id);

        b.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(256);

        b.Property(x => x.ReplacedByTokenHash)
            .HasMaxLength(256);

        b.Property(x => x.CreatedByIp)
            .HasMaxLength(64);

        b.Property(x => x.UserAgent)
            .HasMaxLength(512);

        b.HasIndex(x => x.TokenHash).IsUnique();
        b.HasIndex(x => x.AuthUserId);

        b.HasOne<AuthUser>()
            .WithMany(x => x.AuthRefreshTokens)
            .HasForeignKey(x => x.AuthUserId);
        // indexler
        b.HasIndex(x => x.ExpiresAt);
        b.HasIndex(x => x.RevokedAt);

    }
}
