using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Otomasyon.Domain.Entities;

namespace Otomasyon.DataAccess.Configurations;

public class AuthRoleConfig:IEntityTypeConfiguration<AuthRole>
{
    public void Configure(EntityTypeBuilder<AuthRole> builder)
    {
        builder.ToTable("auth_roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IsDeleted).HasDefaultValue(false);
        builder.HasQueryFilter(x => !x.IsDeleted);
    }
}