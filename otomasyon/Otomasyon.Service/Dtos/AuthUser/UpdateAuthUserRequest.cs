namespace Otomasyon.Service.Dtos.AuthUser;

public sealed class UpdateAuthUserRequest
{
    public Guid Id { get; set; }
    public string Email { get; init; } = null!;
    public string UserName  { get; init; } = null!;
}