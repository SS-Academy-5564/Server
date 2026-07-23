using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.DefaultOrganization;

namespace Pulse.API.Features.DefaultOrganization;

[ApiController]
[Route("api/organizations")]
[Authorize]
public class DefaultOrganizationController : PulseControllerBase
{
    private readonly IAsyncQueryHandler<Result<GetDefaultOrganizationResult>> _handler;

    public DefaultOrganizationController(
        IAsyncQueryHandler<Result<GetDefaultOrganizationResult>> handler)
    {
        _handler = handler;
    }

    [HttpGet("default")]
    public async Task<IActionResult> GetDefaultOrganizationAsync(
        CancellationToken ct)
    {
        Result<GetDefaultOrganizationResult> result =
            await _handler.HandleAsync(ct);

        return ToActionResult(result);
    }
}
