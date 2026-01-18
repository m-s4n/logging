using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Otomasyon.DataAccess.Logs;
using Otomasyon.Domain.Entities;

namespace Otomasyon.API.Logs.Audit;

public sealed class AuditOutboxSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _http;
    private readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

    // Interceptor kendi eklediği outbox'ı tekrar audit'lemesin / recursion olmasın
    private static readonly AsyncLocal<bool> _isRunning = new();

    public AuditOutboxSaveChangesInterceptor(IHttpContextAccessor http)
    {
        _http = http;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null) AddAuditOutboxRows(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null) AddAuditOutboxRows(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private void AddAuditOutboxRows(DbContext db)
    {
        // recursion / re-entrancy guard
        if (_isRunning.Value) return;

        try
        {
            _isRunning.Value = true;

            db.ChangeTracker.DetectChanges();

            var http = _http.HttpContext;
            var now = DateTimeOffset.UtcNow;

            // ✅ UserId (Guid?) - claim'den al (NameIdentifier -> sub fallback)
            Guid? userId = null;
            var idStr =
                http?.User?.FindFirstValue(ClaimTypes.NameIdentifier) ??
                http?.User?.FindFirstValue("sub") ??
                http?.User?.FindFirstValue("userId");

            if (Guid.TryParse(idStr, out var g))
                userId = g;

            var traceId = http?.TraceIdentifier;
            var ip = http?.Connection.RemoteIpAddress?.ToString();
            var userAgent = http?.Request.Headers.UserAgent.ToString();
            var method = http?.Request.Method;
            var path = http?.Request.Path.ToString();
            var queryString = http?.Request.QueryString.HasValue == true ? http!.Request.QueryString.Value : null;

            // ✅ SNAPSHOT: enumerate sırasında change-tracker değişmesin
            var entries = db.ChangeTracker
                .Entries()
                .Where(ShouldAudit)
                .ToList();

            var outboxes = new List<AuditOutbox>(capacity: Math.Min(entries.Count, 64));

            foreach (var entry in entries)
            {
                var entityName = entry.Metadata.ClrType.Name;

                // ✅ recursion / self-audit engeli
                if (entry.Entity is AuditOutbox) continue;
                if (entityName == nameof(AuditOutbox)) continue;

                if (!AuditEntityRegistry.IsAuditable(entityName)) continue;

                var entityId = ResolveEntityId(entry);
                if (string.IsNullOrWhiteSpace(entityId)) continue;

                // ✅ soft delete action desteği burada
                var action = ResolveAction(entry, entityName);

                var diff = BuildDiff(entry);
                if (diff is null) continue;

                outboxes.Add(new AuditOutbox
                {
                    CreatedAt = now,
                    UserId = userId,
                    Action = action,
                    EntityName = entityName,
                    EntityId = entityId,
                    ChangesJson = JsonSerializer.Serialize(diff, _json),
                    TraceId = traceId,
                    IpAddress = ip,
                    UserAgent = userAgent,
                    Method = method,
                    Path = path,
                    QueryString = queryString,
                    IsDispatched = false,
                    DispatchedAt = null
                });
            }

            if (outboxes.Count > 0)
                db.Set<AuditOutbox>().AddRange(outboxes);
        }
        finally
        {
            _isRunning.Value = false;
        }
    }

    private static bool ShouldAudit(EntityEntry e)
        => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted;

    /// <summary>
    /// EF state'e göre action üretir.
    /// Soft delete (IsDeleted true) durumunda Modified yerine Deleted yazar.
    /// </summary>
    private static string ResolveAction(EntityEntry e, string entityName)
    {
        // ✅ Soft delete: Modified ama IsDeleted true olduysa => Deleted
        if (e.State == EntityState.Modified)
        {
            var isDeletedProp = e.Properties.FirstOrDefault(p => p.Metadata.Name == "IsDeleted");

            if (isDeletedProp is not null &&
                isDeletedProp.IsModified &&
                isDeletedProp.CurrentValue is bool b &&
                b == true)
            {
                return $"{entityName}.Deleted";
            }
        }

        return e.State switch
        {
            EntityState.Added => $"{entityName}.Created",
            EntityState.Modified => $"{entityName}.Updated",
            EntityState.Deleted => $"{entityName}.Deleted",
            _ => $"{entityName}.Changed"
        };
    }

    private static string? ResolveEntityId(EntityEntry e)
    {
        var pk = e.Metadata.FindPrimaryKey();
        if (pk is null) return null;

        var parts = new List<string>();
        foreach (var p in pk.Properties)
        {
            var prop = e.Property(p.Name);
            var v = prop.CurrentValue ?? prop.OriginalValue;
            parts.Add(v?.ToString() ?? "NULL");
        }

        return parts.Count == 1 ? parts[0] : string.Join(":", parts);
    }

    private static object? BuildDiff(EntityEntry e)
    {
        static bool IsAllowed(PropertyEntry p)
        {
            if (p.Metadata.IsShadowProperty()) return false;
            if (p.Metadata.IsConcurrencyToken) return false;

            var name = p.Metadata.Name;

            // güvenlik: asla audit'e yazma
            if (name.Contains("Password", StringComparison.OrdinalIgnoreCase)) return false;
            if (name.Contains("Token", StringComparison.OrdinalIgnoreCase)) return false;
            if (name.Contains("Secret", StringComparison.OrdinalIgnoreCase)) return false;

            return true;
        }

        if (e.State == EntityState.Added)
        {
            var d = new Dictionary<string, object?>();
            foreach (var p in e.Properties.Where(IsAllowed).ToList()) // snapshot
                d[p.Metadata.Name] = new { old = (object?)null, @new = Normalize(p.CurrentValue) };
            return d.Count == 0 ? null : d;
        }

        if (e.State == EntityState.Deleted)
        {
            var d = new Dictionary<string, object?>();
            foreach (var p in e.Properties.Where(IsAllowed).ToList()) // snapshot
                d[p.Metadata.Name] = new { old = Normalize(p.OriginalValue), @new = (object?)null };
            return d.Count == 0 ? null : d;
        }

        var changes = new Dictionary<string, object?>();
        foreach (var p in e.Properties.Where(IsAllowed).ToList()) // snapshot
        {
            if (!p.IsModified) continue;

            var oldVal = Normalize(p.OriginalValue);
            var newVal = Normalize(p.CurrentValue);

            if (Equals(oldVal, newVal)) continue;

            changes[p.Metadata.Name] = new { old = oldVal, @new = newVal };
        }

        return changes.Count == 0 ? null : changes;
    }

    private static object? Normalize(object? v)
        => v switch
        {
            DateTime dt => DateTime.SpecifyKind(dt, DateTimeKind.Utc).ToString("O"),
            DateTimeOffset dto => dto.ToUniversalTime().ToString("O"),
            Guid g => g.ToString(),
            Enum e => e.ToString(),
            decimal d => d.ToString(System.Globalization.CultureInfo.InvariantCulture),
            _ => v
        };
}
