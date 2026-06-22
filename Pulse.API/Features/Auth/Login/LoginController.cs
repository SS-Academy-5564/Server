using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pulse.API.Attributes;
using Pulse.API.Constants;
using Pulse.BL.Features.Auth.Login;

namespace Pulse.API.Features.Auth.Login;

[ApiController]
[Route("api/auth")]
[AutoValidate]
public class LoginController : Controllers.PulseControllerBase
{
    private readonly ILoginHandler _handler;

    public LoginController(ILoginHandler handler)
    {
        _handler = handler;
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<IActionResult> LoginAsync([Validate] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _handler.LoginAsync(command, ct);
        return ToActionResult(result);
    }
}
