using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Otomasyon.Domain.Entities;

namespace Otomasyon.DataAccess.Configurations;

public class AuthUserRoleConfig:IEntityTypeConfiguration<AuthUserRole>
{
    public void Configure(EntityTypeBuilder<AuthUserRole> builder)
    {
        builder.ToTable("auth_user_roles");

        // Join tabloda genelde composite key
        builder.HasKey(x => new { x.UserId, x.RoleId });

        builder.Property(x => x.UserId).HasColumnType("uuid");
        builder.Property(x => x.RoleId).HasColumnType("uuid");

        builder.HasOne(x => x.AuthUser)
            .WithMany(u => u.AuthUserRoles)
            .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.AuthRole)
            .WithMany(r => r.AuthUserRoles)
            .HasForeignKey(x => x.RoleId);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.RoleId);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}