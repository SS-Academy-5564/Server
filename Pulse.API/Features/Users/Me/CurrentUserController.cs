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
    private readonly IGetCurrentUserQueryHandler _query;

    public CurrentUserController(IGetCurrentUserQueryHandler query)
    {
        _query = query;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken ct)
    {
        return ToActionResult(await _query.HandleAsync(ct));
    }
}
