using Microsoft.AspNetCore.Mvc;
using Otomasyon.Service.Dtos.Auth;
using Otomasyon.Service.Services.Auth;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Otomasyon.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController(IAuthService authService):ControllerBase
{
    [HttpPost(template: ("login"))]
    public async Task<IActionResult> Login([FromBody] LoginAuthRequest request)
    {
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        var userAgent = Request.Headers.UserAgent.ToString();
        
        var (access, refresh) = await authService.LoginAsync(request.Email, request.Password, ip, userAgent);

        SetRefreshCookie(refresh);
        return Ok(new LoginAuthResponse(access));
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<LoginAuthResponse>> Refresh()
    {
        var refresh = Request.Cookies["refresh_token"];
        if (string.IsNullOrWhiteSpace(refresh))
            return Unauthorized();

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var ua = Request.Headers.UserAgent.ToString();

        var (access, newRefresh) = await authService.RefreshAsync(refresh, ip, ua);

        SetRefreshCookie(newRefresh);
        return Ok(new LoginAuthResponse(access));
    }
    
    [Authorize]
    [HttpPost("logout-all")]
    public async Task<IActionResult> LogoutAll()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userIdStr))
            return Unauthorized();

        await authService.LogoutAllAsync(Guid.Parse(userIdStr));

        Response.Cookies.Delete("refresh_token");
        return NoContent();
    }
    
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        var refresh = Request.Cookies["refresh_token"];
        if (!string.IsNullOrWhiteSpace(refresh))
            await authService.LogoutAsync(refresh);

        Response.Cookies.Delete("refresh_token");
        return NoContent();
    }
    
    
    
    private void SetRefreshCookie(string refresh)
    {
        Response.Cookies.Append("refresh_token", refresh, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(14),
            Path = "/auth/refresh"
        });
    }
}