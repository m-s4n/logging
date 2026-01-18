using Microsoft.AspNetCore.Mvc;
using Otomasyon.API.Common.Responses;
using Otomasyon.Service.Dtos.AuthRole;
using Otomasyon.Service.Services.AuthRole;

namespace Otomasyon.API.Controllers;

[ApiController]
[Route("api/v1/auth-roles")]
public class AuthRolesController(IAuthRoleService authRoleService):ControllerBase
{
    [HttpGet(template: "get-roles")]
    public async Task<IActionResult> GetRoles(CancellationToken ct)
    {
        var roles = await authRoleService
            .GetAllAuthRolesAsync(ct);

        return Ok(ApiResponses.Success(data: roles));
    }

    [HttpPost(template: "add-role")]
    public async Task<IActionResult> AddRole([FromBody] CreateAuthRoleRequest request, CancellationToken ct)
    {
        var role = await authRoleService.CreateAuthRoleAsync(request, ct);
        return Ok(ApiResponses.Success(data: role, statusCode: 201));
    }

    [HttpPut(template: "update-role/{id:guid}")]
    public async Task<IActionResult> UpdateRole([FromBody] UpdateAuthRoleRequest request, CancellationToken ct, [FromRoute] Guid id)
    {
        request.Id = id;
        var result = await authRoleService
            .UpdateAuthRoleAsync(request, ct); 
        return Ok(ApiResponses.Success(data:result));
    }
    [HttpDelete(template: "delete-user/{id:guid}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id, CancellationToken ct)
    {
        await authRoleService.DeleteAuthRoleAsync(id,ct);
        return Ok(ApiResponses.Success<object>(message:"Silindi", data: null));
    }
}