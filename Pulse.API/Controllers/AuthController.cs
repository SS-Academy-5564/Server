using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pulse.API.Constants;

namespace Pulse.API.Controllers;

public class AuthController : ControllerBase
{
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public Task<IActionResult> Login()
    {
        throw new NotImplementedException();
    }
}
