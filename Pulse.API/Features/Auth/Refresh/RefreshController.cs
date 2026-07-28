using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Pulse.API.Attributes;
using Pulse.API.Common;
using Pulse.API.Controllers;
using Pulse.API.Features.Auth.Login;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.Login;
using Pulse.BL.Features.Auth.Refresh;

namespace Pulse.API.Features.Auth.Refresh;

[ApiController]
[Route("api/auth")]
[AutoValidate]
public class RefreshController : PulseControllerBase
{
    private readonly IAsyncHandler<RefreshCommand, Result<LoginResult>> _handler;
    private readonly RefreshTokenOptions _refreshTokenOptions;
    private readonly TimeProvider _timeProvider;

    public RefreshController(
        IAsyncHandler<RefreshCommand, Result<LoginResult>> handler,
        IOptions<RefreshTokenOptions> refreshTokenOptions,
        TimeProvider timeProvider)
    {
        _handler = handler;
        _refreshTokenOptions = refreshTokenOptions.Value;
        _timeProvider = timeProvider;
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshAsync(CancellationToken ct)
    {
        string? refreshToken = Request.Cookies[CookieConstants.RefreshTokenCookieName];
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        RefreshCommand command = new(refreshToken);
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

            return ToActionResult(Result.Ok(new LoginResponse(result.Value.AccessToken, result.Value.ExpiresAt)));
        }

        // Ensure invalid tokens return 401 instead of generic errors based on mapping
        return Unauthorized();
    }
}
