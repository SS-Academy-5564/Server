using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pulse.API.Constants;
using Pulse.API.Controllers;
using Pulse.BL.Features.Polling.ManualTrigger;

namespace Pulse.API.Features.Monitors.TriggerMonitorCheck;

[ApiController]
[Route("api/monitors")]
public sealed class TriggerMonitorCheckController : PulseControllerBase
{
    private readonly IManualMonitorTriggerService _manualTriggerService;

    public TriggerMonitorCheckController(IManualMonitorTriggerService manualTriggerService)
    {
        _manualTriggerService = manualTriggerService;
    }

    [HttpPost("{id:guid}/run-now")]
    [EnableRateLimiting(RateLimitPolicies.ManualMonitorTrigger)]
    public async Task<IActionResult> RunNowAsync(Guid id, CancellationToken ct)
    {
        Result result = await _manualTriggerService.TriggerAsync(id, ct);
        return ToActionResult(result);
    }
}
