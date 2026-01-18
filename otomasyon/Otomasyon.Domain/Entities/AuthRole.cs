namespace Otomasyon.Domain.Entities;

public sealed class AuthRole:BaseEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<AuthUserRole> AuthUserRoles { get; set; } = new HashSet<AuthUserRole>();
}