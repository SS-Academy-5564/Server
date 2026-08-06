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

/// <summary>
/// Provides account login and refresh-cookie issuance.
/// </summary>
[ApiController]
[Route("api/auth")]
[AutoValidate]
public class LoginController : Controllers.PulseControllerBase
{
    private readonly IAsyncHandler<LoginCommand, Result<LoginResult>> _handler;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly TimeProvider _timeProvider;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoginController"/> class.
    /// </summary>
    /// <param name="handler">The handler for processing login commands.</param>
    /// <param name="refreshTokenOptions">The refresh-token lifetime configuration.</param>
    /// <param name="timeProvider">The provider for current time.</param>
    /// <param name="environment">The current hosting environment.</param>
    public LoginController(
        IAsyncHandler<LoginCommand, Result<LoginResult>> handler,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        TimeProvider timeProvider,
        IHostEnvironment environment)
    {
        _handler = handler;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _timeProvider = timeProvider;
        _environment = environment;
    }

    /// <summary>
    /// Authenticates a user and returns an access token.
    /// </summary>
    /// <param name="request">The login payload containing email and password.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>200 OK with login result (e.g., JWT token) on success, or an error response on failure.</returns>
    [HttpPost("login")]
    [EnableRateLimiting(RateLimitPolicies.Login)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LoginAsync([Validate] LoginRequest request, CancellationToken ct)
    {
        string identifier = Request.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
        LoginCommand command = new(request.Email, request.Password, identifier);
        Result<LoginResult> result = await _handler.HandleAsync(command, ct);

        if (result.IsSuccess)
        {
            CookieOptions cookieOptions = RefreshTokenCookieFactory.Create(
                Request,
                _environment,
                _timeProvider.GetUtcNow().AddDays(_refreshTokenOptions.ExpirationDays));

            Response.Cookies.Append(CookieConstants.RefreshTokenCookieName, result.Value.RefreshToken, cookieOptions);
        }

        return ToActionResult(result.Map(r => new LoginResponse(r.AccessToken, r.ExpiresAt)));
    }
}
