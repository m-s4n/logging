namespace Otomasyon.Service.Dtos.AuthUser;

public sealed record AuthUserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = null!;
    public string Email { get; init; } = null!;
}