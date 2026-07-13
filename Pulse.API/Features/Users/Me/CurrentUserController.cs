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
    private readonly IAsyncQueryHandler<Result<IReadOnlyList<UserOrganizationResult>>> _organizationsQuery;

    public CurrentUserController(
        IAsyncQueryHandler<Result<UserProfileResult>> query,
        IAsyncQueryHandler<Result<IReadOnlyList<UserOrganizationResult>>> organizationsQuery)
    {
        _query = query;
        _organizationsQuery = organizationsQuery;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken ct)
    {
        return ToActionResult(await _query.HandleAsync(ct));
    }

    [HttpGet("me/organizations")]
    public async Task<IActionResult> GetCurrentUserOrganizationsAsync(CancellationToken ct)
    {
        return ToActionResult(await _organizationsQuery.HandleAsync(ct));
    }
}
