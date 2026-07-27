using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Pulse.BL.Common.Security;
using Pulse.BL.Common.Security.Tokens;

namespace Pulse.API.Common.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            try
            {
                Claim? sub = _httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)
                            ?? _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
                return sub is not null && Guid.TryParse(sub.Value, out Guid id) ? id : null;
            }
            catch
            {
                return null;
            }
        }
    }

    public string Role => GetClaim("role") ?? throw new InvalidOperationException("Missing role claim.");

    public Guid? OrganizationId
    {
        get
        {
            string? value = GetClaim(JwtClaimNames.OrganizationId);

            return value is not null && Guid.TryParse(value, out Guid id)
                ? id
                : null;
        }
    }

    private string? GetClaim(string type) => _httpContextAccessor.HttpContext?.User.FindFirst(type)?.Value;
}
