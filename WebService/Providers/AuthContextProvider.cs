using System.Security.Claims;
using WebService.Providers.Interfaces;

namespace WebService.Providers;

public class AuthContextProvider(IHttpContextAccessor httpContextAccessor) : IAuthContextProvider
{
    public string? GetUserId() =>
        httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

    public string? GetUserEmail() =>
        httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;

    public string? GetUserName() =>
        httpContextAccessor.HttpContext?.User.FindFirst(c => c.Type == ClaimTypes.Name)?.Value;
}