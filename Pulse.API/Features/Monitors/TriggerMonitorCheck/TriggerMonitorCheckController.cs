using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pulse.API.Constants;
using Pulse.API.Controllers;
using Pulse.API.Responses;
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
        FluentResults.Result result = await _manualTriggerService.TriggerAsync(id, ct);

        if (result.IsFailed)
        {
            bool notFound = result.Errors.Any(e =>
                e.Metadata.TryGetValue("Code", out object? code) &&
                code as string == ManualMonitorTriggerService.MonitorNotFoundErrorCode);

            if (notFound)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Errors = [new ApiError { Code = "MonitorNotFound", Message = "Monitor not found." }]
                });
            }

            return StatusCode(500, new ApiResponse
            {
                Success = false,
                Errors = [new ApiError { Code = "TriggerFailed", Message = "Failed to trigger monitor check." }]
            });
        }

        return Accepted(new ApiResponse { Success = true });
    }
}
