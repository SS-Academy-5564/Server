using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Users.Me;

namespace Pulse.API.Features.Users.Me;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class CurrentUserController : PulseControllerBase
{
    private readonly IAsyncQueryHandler<Result<UserProfileResult>> _query;

    public CurrentUserController(IAsyncQueryHandler<Result<UserProfileResult>> query)
    {
        _query = query;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken ct)
    {
        return ToActionResult(await _query.HandleAsync(ct));
    }
}
