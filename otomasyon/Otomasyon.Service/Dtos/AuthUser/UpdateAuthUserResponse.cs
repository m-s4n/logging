namespace Otomasyon.Service.Dtos.AuthUser;

public sealed class UpdateAuthUserResponse
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
}