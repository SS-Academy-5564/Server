using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pulse.API.Constants;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Polling.ManualCheck;

namespace Pulse.API.Features.Monitors.TriggerMonitorCheck;

[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class TriggerMonitorCheckController : PulseControllerBase
{
    private readonly IAsyncHandler<ManualCheckCommand, Result> _handler;
    private readonly ICurrentUserService _userService;

    public TriggerMonitorCheckController(IAsyncHandler<ManualCheckCommand, Result> handler, ICurrentUserService userService)
    {
        _handler = handler;
        _userService = userService;
    }

    /// <summary>
    /// Queues an immediate check for the specified monitor.
    /// </summary>
    /// <param name="id">The identifier of the monitor to check.</param>
    /// <param name="ct">The cancellation token for the request.</param>
    /// <returns>An HTTP response indicating whether the monitor check was queued.</returns>
    /// <exception cref="OperationCanceledException">The request is canceled.</exception>
    [HttpPost("{id:guid}/run-now")]
    [EnableRateLimiting(RateLimitPolicies.ManualMonitorTrigger)]
    public async Task<IActionResult> RunNowAsync(Guid id, CancellationToken ct)
    {
        Result<Guid> organizationId = _userService.RequireOrganizationId();
        if (organizationId.IsFailed)
        {
            return ToActionResult(organizationId);
        }

        Result result = await _handler.HandleAsync(new ManualCheckCommand(id, organizationId.Value), ct);

        return ToActionResult(result);
    }
}
