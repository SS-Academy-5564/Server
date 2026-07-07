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

    public Guid UserId => Guid.TryParse(GetClaim(ClaimTypes.NameIdentifier) ?? GetClaim("sub"), out Guid id)
        ? id
        : throw new InvalidOperationException("Missing or invalid user id claim.");

    public string Role => GetClaim("role") ?? throw new InvalidOperationException("Missing role claim.");

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
