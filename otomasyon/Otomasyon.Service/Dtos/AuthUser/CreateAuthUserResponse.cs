namespace Otomasyon.Service.Dtos.AuthUser;

public sealed record CreateAuthUserResponse
{
    public Guid Id { get; init; }
    public string Username { get; init; } = null!;
    public string Email { get; init; }  = null!;
}