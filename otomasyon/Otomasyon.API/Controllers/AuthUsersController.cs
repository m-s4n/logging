using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Otomasyon.API.Common.Responses;
using Otomasyon.Service.Services.AuthUser;
using Otomasyon.Service.Dtos.AuthUser;

namespace Otomasyon.API.Controllers;


[Route("api/v1/auth-users")]
[ApiController]
public class AuthUsersController(IAuthUserService authUserService):ControllerBase
{
    [Authorize(Roles = "ADMIN")]
    [HttpGet(template: "get-users")]
    public async Task<IActionResult> GetUsers()
    {
        var result = await authUserService.GetAllAuthUsersAsync();
        return Ok(ApiResponses.Success(data:result));
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost(template: "add-user")]
    public async Task<IActionResult> AddUser([FromBody] CreateAuthUserRequest request)
    {
        var result = await authUserService.CreateAuthUserAsync(request);
        return Ok(ApiResponses.Success(data:result, statusCode: 201, message:"Oluşturuldu"));
    }

    [HttpPut(template: "update-user/{id:guid}")]
    public async Task<IActionResult> UpdateUser([FromRoute] Guid id,[FromBody] UpdateAuthUserRequest request)
    {
        request.Id = id;
        var result = await authUserService.UpdateAuthUserAsync(request);
        return Ok(ApiResponses.Success(data:result));
    }

    [HttpDelete(template: "delete-user/{id:guid}")]
    public async Task<IActionResult> DeleteUser([FromRoute] Guid id)
    {
        await authUserService.DeleteAuthUserAsync(id);
        return Ok(ApiResponses.Success<object>(message:"Silindi", data: null));
    }
}