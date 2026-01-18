using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Otomasyon.Domain.Entities;

namespace Otomasyon.DataAccess.Configurations;

public class AuthUserConfig:IEntityTypeConfiguration<AuthUser>
{
    public void Configure(EntityTypeBuilder<AuthUser> builder)
    {
        builder.ToTable("auth_users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Email).HasMaxLength(256);
        builder.HasQueryFilter(x => !x.IsDeleted);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
    }
}