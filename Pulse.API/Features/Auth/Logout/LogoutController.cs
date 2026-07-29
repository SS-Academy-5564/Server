using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Common;
using Pulse.API.Controllers;
using Pulse.API.Responses;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.Logout;

namespace Pulse.API.Features.Auth.Logout;

[ApiController]
[Route("api/auth")]
public class LogoutController : PulseControllerBase
{
    private readonly IAsyncHandler<LogoutCommand, Result> _handler;

    public LogoutController(IAsyncHandler<LogoutCommand, Result> handler)
    {
        _handler = handler;
    }

    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> LogoutAsync(CancellationToken ct)
    {
        string? refreshToken = Request.Cookies[CookieConstants.RefreshTokenCookieName];

        LogoutCommand command = new(refreshToken);
        Result result = await _handler.HandleAsync(command, ct);

        Response.Cookies.Delete(CookieConstants.RefreshTokenCookieName, new CookieOptions
        {
            HttpOnly = true,
            Secure = Request.IsHttps,
            SameSite = SameSiteMode.Lax
        });

        return ToActionResult(result);
    }
}
