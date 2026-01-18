using Otomasyon.Shared.Services;
using System.Security.Claims;
namespace Otomasyon.API.Services;

public class CurrentUserServices(IHttpContextAccessor httpContextAccessor):ICurrentUserService
{
    public Guid? UserId
    {
        get
        {
             var user = httpContextAccessor.HttpContext?.User;
             if (user?.Identity?.IsAuthenticated != true) return null;
             
             var id = user.FindFirstValue(ClaimTypes.NameIdentifier);
             if(string.IsNullOrWhiteSpace(id)) return null;
             
             return Guid.Parse(id);
        }
    }

    public bool IsAuthenticated => 
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;
}