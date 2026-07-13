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

    public Guid? OrganizationId
    {
        get
        {
            Claim? organizationId =
                _httpContextAccessor.HttpContext?.User.FindFirst(JwtClaimNames.OrganizationId);

            return organizationId is not null &&
                Guid.TryParse(organizationId.Value, out Guid id)
                ? id
                : null;
        }
    }
}
