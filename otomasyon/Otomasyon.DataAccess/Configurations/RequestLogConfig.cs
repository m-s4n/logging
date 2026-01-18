using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Otomasyon.Domain.Entities;

namespace Otomasyon.DataAccess.Configurations;

public class RequestLogConfig:IEntityTypeConfiguration<RequestLog>
{
    public void Configure(EntityTypeBuilder<RequestLog> builder)
    {
        builder.ToTable("request_logs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("now()");

        builder.Property(x => x.IsDispatched)
            .HasDefaultValue(false);

        builder.Property(x => x.DispatchedAt)
            .IsRequired(false);

        builder.Property(x => x.Method).IsRequired();
        builder.Property(x => x.Path).IsRequired();
        builder.Property(x => x.Level).IsRequired();
        builder.Property(x => x.TraceId).IsRequired();

        builder.Property(x => x.QueryString).IsRequired(false);
        builder.Property(x => x.ErrorCode).IsRequired(false);
        builder.Property(x => x.ErrorMessage).IsRequired(false);
        builder.Property(x => x.IpAddress).IsRequired(false);

        // Worker için faydalı index
        builder.HasIndex(x => new { x.IsDispatched, x.CreatedAt });
    }
}