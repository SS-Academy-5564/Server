using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Features.Users.Me;

namespace Pulse.API.Features.Users.Me;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class CurrentUserController : PulseControllerBase
{
    private readonly ICurrentUserHandler _handler;

    public CurrentUserController(ICurrentUserHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken ct)
    {
        return ToActionResult(await _handler.GetCurrentUserAsync(ct));
    }
}
