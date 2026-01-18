namespace Otomasyon.Service.Dtos.AuthRole;

public sealed record AuthRoleDto
{
    public Guid Id { get; init; } =  Guid.Empty!;
    public string Name { get; init; } = null!;
}