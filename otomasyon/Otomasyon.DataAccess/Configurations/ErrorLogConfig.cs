using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Otomasyon.Domain.Entities;

namespace Otomasyon.DataAccess.Configurations;

public class ErrorLogConfig:IEntityTypeConfiguration<ErrorLog>
{
    public void Configure(EntityTypeBuilder<ErrorLog> builder)
    {
        builder.ToTable("error_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.IsDispatched)
            .HasDefaultValue(false);

        builder.Property(x => x.DispatchedAt)
            .IsRequired(false);

        builder.Property(x => x.Method).IsRequired();
        builder.Property(x => x.Path).IsRequired();
        builder.Property(x => x.TraceId).IsRequired();
        builder.Property(x => x.ExceptionType).IsRequired();
        builder.Property(x => x.StackTrace).IsRequired();

        builder.Property(x => x.QueryString).IsRequired(false);
        builder.Property(x => x.ErrorCode).IsRequired(false);
        builder.Property(x => x.ErrorMessage).IsRequired(false);
        builder.Property(x => x.IpAddress).IsRequired(false);
        builder.Property(x => x.InnerException).IsRequired(false);

        builder.HasIndex(x => new { x.IsDispatched, x.CreatedAt });
    }
}