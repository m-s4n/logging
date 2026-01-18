using Otomasyon.Service.Dtos.AuthRole;

namespace Otomasyon.Service.Services.AuthRole;
public interface IAuthRoleService
{
    Task<CreateAuthRoleResponse> CreateAuthRoleAsync(CreateAuthRoleRequest request, CancellationToken ct = default);
    Task<List<AuthRoleDto>> GetAllAuthRolesAsync(CancellationToken ct = default);
    Task<UpdateAuthRoleResponse> UpdateAuthRoleAsync(UpdateAuthRoleRequest request, CancellationToken ct = default);
    Task DeleteAuthRoleAsync(Guid id, CancellationToken ct = default);
}