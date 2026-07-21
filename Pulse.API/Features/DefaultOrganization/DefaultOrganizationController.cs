using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Features.DefaultOrganization;

namespace Pulse.API.Features.DefaultOrganization;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class DefaultOrganizationController : PulseControllerBase
{
    private readonly GetDefaultOrganizationHandler _handler;

    public DefaultOrganizationController(
        GetDefaultOrganizationHandler handler)
    {
        _handler = handler;
    }

    [HttpGet("default")]
    public async Task<IActionResult> GetDefaultOrganizationAsync(
        CancellationToken ct)
    {
        Result<GetDefaultOrganizationResult> result =
            await _handler.HandleAsync(new GetDefaultOrganizationQuery(), ct);

        return ToActionResult(result);
    }
}
