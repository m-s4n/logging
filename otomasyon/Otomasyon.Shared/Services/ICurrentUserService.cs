namespace Otomasyon.Shared.Services;

public interface ICurrentUserService
{
    Guid? UserId { get; }
    bool IsAuthenticated { get;}
}