namespace Otomasyon.Domain.Entities;

public class AuthUserRole: BaseEntity
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public AuthUser AuthUser { get; set; } = null!;
    public AuthRole AuthRole { get; set; } = null!;
}