using System.Security.Claims;
using JewelryHub.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace JewelryHub.Infrastructure.Security;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            // ASP.NET Core's default JWT handler maps "sub" onto the long
            // ClaimTypes.NameIdentifier URI unless MapInboundClaims is
            // disabled in Program.cs (this project disables it, see
            // JewelryHub.API/Program.cs), so checking both keeps this
            // resilient either way.
            var sub = User?.FindFirstValue("sub") ?? User?.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue("email") ?? User?.FindFirstValue(ClaimTypes.Email);

    public IReadOnlyCollection<string> Roles =>
        User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? Array.Empty<string>();

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
