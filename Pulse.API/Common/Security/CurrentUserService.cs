using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using Pulse.BL.Common.Security;

namespace Pulse.API.Common.Security;

public sealed class CurrentUserService : ICurrentUserService
{
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        Claim? sub = httpContextAccessor.HttpContext?.User.FindFirst(JwtRegisteredClaimNames.Sub)
                    ?? httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
        UserId = sub is not null && Guid.TryParse(sub.Value, out Guid id) ? id : null;
    }

    public Guid? UserId { get; }
}
