namespace Otomasyon.Service.Dtos.AuthRole;

public sealed class UpdateAuthRoleRequest
{
    public Guid Id { get; set; }
    public string Name { get; init; } = null!;
}