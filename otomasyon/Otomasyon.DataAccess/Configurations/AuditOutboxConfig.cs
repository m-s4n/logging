using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Otomasyon.Domain.Entities;

namespace Otomasyon.DataAccess.Configurations;

public sealed class AuditOutboxConfiguration : IEntityTypeConfiguration<AuditOutbox>
{
    public void Configure(EntityTypeBuilder<AuditOutbox> b)
    {
        b.ToTable("audit_outbox");

        b.HasKey(x => x.Id);

        b.Property(x => x.Id)
            .ValueGeneratedOnAdd();

        // Guid? -> uuid
        b.Property(x => x.UserId)
            .HasColumnType("uuid");

        b.Property(x => x.Action)
            .IsRequired()
            .HasMaxLength(128);

        b.Property(x => x.EntityName)
            .IsRequired()
            .HasMaxLength(128);

        b.Property(x => x.EntityId)
            .IsRequired()
            .HasMaxLength(128);

        // jsonb
        b.Property(x => x.ChangesJson)
            .IsRequired()
            .HasColumnType("jsonb");

        b.Property(x => x.TraceId).HasMaxLength(128);
        b.Property(x => x.IpAddress).HasMaxLength(64);
        b.Property(x => x.UserAgent).HasMaxLength(512);
        b.Property(x => x.Method).HasMaxLength(16);
        b.Property(x => x.Path).HasMaxLength(512);
        b.Property(x => x.QueryString).HasMaxLength(2048);

        b.Property(x => x.CreatedAt)
            .IsRequired();

        b.Property(x => x.IsDispatched)
            .IsRequired()
            .HasDefaultValue(false);

        b.Property(x => x.DispatchedAt);

        // Outbox worker için en kritik index
        b.HasIndex(x => new { x.IsDispatched, x.Id });

        // Listeleme / retention işleri için (opsiyonel ama faydalı)
        b.HasIndex(x => x.CreatedAt);
    }
}
