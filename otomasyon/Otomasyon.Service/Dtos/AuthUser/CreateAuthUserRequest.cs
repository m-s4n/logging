namespace Otomasyon.Service.Dtos.AuthUser;

public sealed record CreateAuthUserRequest
{
    public string Username { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string Password { get; init; }  = null!;
}