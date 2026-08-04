using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Pulse.API.Common;
using Pulse.API.Constants;
using Pulse.API.Controllers;
using Pulse.API.Features.Auth.Login;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login;
using Pulse.BL.Features.Auth.Refresh;

namespace Pulse.API.Features.Auth.Refresh;

/// <summary>
/// Provides endpoints for refreshing authentication tokens.
/// </summary>
[ApiController]
[Route("api/auth")]
public class RefreshController : PulseControllerBase
{
    private readonly IAsyncHandler<RefreshCommand, Result<LoginResult>> _handler;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly TimeProvider _timeProvider;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshController"/> class.
    /// </summary>
    /// <param name="handler">The handler for processing the refresh command.</param>
    /// <param name="refreshTokenOptions">The configuration options for refresh tokens.</param>
    /// <param name="timeProvider">The provider for time-related operations.</param>
    /// <param name="environment">The current hosting environment.</param>
    public RefreshController(
        IAsyncHandler<RefreshCommand, Result<LoginResult>> handler,
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
    /// Refreshes the user's authentication token using a refresh token from cookies.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>200 OK with new login result on success, or an error response on failure.</returns>
    [HttpPost("refresh")]
    [Consumes("application/json")]
    [EnableRateLimiting(RateLimitPolicies.Refresh)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> RefreshAsync(CancellationToken ct)
    {
        string? refreshToken = Request.Cookies[CookieConstants.RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return ToActionResult(Result.Fail(new UnauthorizedError("Refresh token is missing.")));
        }

        RefreshCommand command = new(refreshToken);
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
