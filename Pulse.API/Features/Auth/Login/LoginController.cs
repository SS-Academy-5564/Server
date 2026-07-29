using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Pulse.API.Attributes;
using Pulse.API.Common;
using Pulse.API.Constants;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login;

namespace Pulse.API.Features.Auth.Login;

[ApiController]
[Route("api/auth")]
[AutoValidate]
public class LoginController : Controllers.PulseControllerBase
{
    private readonly IAsyncHandler<LoginCommand, Result<LoginResult>> _handler;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly TimeProvider _timeProvider;

    public LoginController(
        IAsyncHandler<LoginCommand, Result<LoginResult>> handler,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        TimeProvider timeProvider)
    {
        _handler = handler;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _timeProvider = timeProvider;
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

        if (result.IsSuccess)
        {
            CookieOptions cookieOptions = new()
            {
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = _timeProvider.GetUtcNow().AddDays(_refreshTokenOptions.ExpirationDays)
            };

            Response.Cookies.Append(CookieConstants.RefreshTokenCookieName, result.Value.RefreshToken, cookieOptions);
        }

        return ToActionResult(result.Map(r => new LoginResponse(r.AccessToken, r.ExpiresAt)));
    }
}
