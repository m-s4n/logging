namespace Otomasyon.Domain.Entities;

public abstract class BaseEntity
{
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; } 
    public Guid? DeletedBy { get; private set; }
    public bool IsDeleted { get; private set; }

    public void SetCreated(Guid? userId)
    {
        CreatedAt = DateTimeOffset.UtcNow;
        CreatedBy = userId;
    }

    public void SetUpdated(Guid? userId)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = userId;
    }

    public void SetDeleted(Guid? userId)
    {
        if (IsDeleted) return;
        DeletedAt = DateTimeOffset.UtcNow;
        DeletedBy = userId;
        IsDeleted = true;
    }
}