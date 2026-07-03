using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Pulse.BL.Common.Security.CurrentUser;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId => Guid.Parse(GetClaim(ClaimTypes.NameIdentifier) ?? GetClaim("sub")!);

    public string Role => GetClaim("role")!;

    public Guid? OrganizationId
    {
        get
        {
            string? value = GetClaim("orgId");
            return string.IsNullOrEmpty(value) ? null : Guid.Parse(value);
        }
    }

    private string? GetClaim(string type)
    {
        return _httpContextAccessor.HttpContext?
            .User?
            .FindFirst(type)?
            .Value;
    }
}
