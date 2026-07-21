using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pulse.API.Attributes;
using Pulse.API.Constants;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.Login;

namespace Pulse.API.Features.Auth.Login;

[ApiController]
[Route("api/auth")]
[AutoValidate]
public class LoginController : Controllers.PulseControllerBase
{
    private readonly IAsyncHandler<LoginCommand, Result<LoginResult>> _handler;
    public LoginController(IAsyncHandler<LoginCommand, Result<LoginResult>> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Authenticates a user and returns an access token.
    /// </summary>
    /// <param name="request">The login payload containing email and password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>200 OK with login result (e.g., JWT token) on success, or an error response on failure.</returns>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    public async Task<IActionResult> LoginAsync([Validate] LoginRequest request, CancellationToken ct)
    {
        LoginCommand command = new(request.Email, request.Password);
        Result<LoginResult> result = await _handler.HandleAsync(command, ct);
        return ToActionResult(result);
    }
}
