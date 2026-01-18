using Otomasyon.Service.Dtos.AuthUser;

namespace Otomasyon.Service.Services.AuthUser;

public interface IAuthUserService
{
    Task<CreateAuthUserResponse> CreateAuthUserAsync(CreateAuthUserRequest request);
    Task<List<AuthUserDto>> GetAllAuthUsersAsync();
    Task<UpdateAuthUserResponse> UpdateAuthUserAsync(UpdateAuthUserRequest request);
    Task DeleteAuthUserAsync(Guid id);
}