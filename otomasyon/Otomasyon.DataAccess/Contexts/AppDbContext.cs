using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Otomasyon.Domain.Entities;
using Otomasyon.Shared.Services;

namespace Otomasyon.DataAccess.Contexts;

public class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserService currentUserService): DbContext(options)
{
    public DbSet<AuthUser> AuthUsers { get; set; }
    public DbSet<AuthUserRole> AuthUserRoles { get; set; }
    public DbSet<AuthRole> AuthRoles { get; set; }
    public DbSet<AuthRefreshToken> AuthRefreshTokens => Set<AuthRefreshToken>();
    
    // loglama
    public DbSet<RequestLog> RequestLogs { get; set; }
    public DbSet<ErrorLog> ErrorLogs { get; set; }
    public DbSet<AuditOutbox> AuditOutboxes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .LogTo(Console.WriteLine, LogLevel.Information)
            .UseSnakeCaseNamingConvention();
        
        base.OnConfiguring(optionsBuilder);
    }

    public override int SaveChanges()
    {
        ApplyEntityStamp();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyEntityStamp();
        return base.SaveChangesAsync(cancellationToken);
    }
    
    private void ApplyEntityStamp()
    {
        Guid? userId = currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Entity.SetCreated(userId);

            if (entry.State == EntityState.Modified && entry.Properties.Any(p => p.IsModified))
                entry.Entity.SetUpdated(userId);

            // ✅ Soft delete otomatiği
            if (entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;   // fiziksel silmeyi engelle
                entry.Entity.SetDeleted(userId);      // IsDeleted / DeletedAt / DeletedBy bas
            }
        }
    }

}