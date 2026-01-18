namespace Otomasyon.Service.Dtos.AuthRole;


public sealed record CreateAuthRoleRequest
{
    public string Name { get; init; } = null!;
}