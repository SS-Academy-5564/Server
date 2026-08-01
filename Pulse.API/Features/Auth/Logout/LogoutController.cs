using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Common;
using Pulse.API.Controllers;
using Pulse.API.Responses;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.Logout;

namespace Pulse.API.Features.Auth.Logout;

/// <summary>
/// Provides endpoints for user logout operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public class LogoutController : PulseControllerBase
{
    private readonly IAsyncHandler<LogoutCommand, Result> _handler;
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogoutController"/> class.
    /// </summary>
    /// <param name="handler">The handler for processing the logout command.</param>
    /// <param name="environment">The current hosting environment.</param>
    public LogoutController(
        IAsyncHandler<LogoutCommand, Result> handler,
        IHostEnvironment environment)
    {
        _handler = handler;
        _environment = environment;
    }

    /// <summary>
    /// Logs out the current user by revoking their refresh token.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A 200 OK response on success.</returns>
    [HttpPost("logout")]
    [Consumes("application/json")]
    public async Task<ActionResult<ApiResponse>> LogoutAsync(CancellationToken ct)
    {
        string? refreshToken = Request.Cookies[CookieConstants.RefreshTokenCookieName];

        LogoutCommand command = new(refreshToken);
        Result result = await _handler.HandleAsync(command, ct);

        Response.Cookies.Delete(
            CookieConstants.RefreshTokenCookieName,
            RefreshTokenCookieFactory.Create(Request, _environment));

        return ToActionResult(result);
    }
}
