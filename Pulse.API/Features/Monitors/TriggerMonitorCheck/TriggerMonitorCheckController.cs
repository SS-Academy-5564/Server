using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.SignalR;
using Pulse.API.Common.Security;
using Pulse.API.Constants;
using Pulse.API.Controllers;
using Pulse.API.Hubs;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;
using Pulse.BL.Features.Polling.ManualCheck;
using Pulse.DAL.Common.Constants;

namespace Pulse.API.Features.Monitors.TriggerMonitorCheck;

[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class TriggerMonitorCheckController : PulseControllerBase
{
    private readonly IAsyncHandler<ManualCheckCommand, Result> _handler;
    private readonly INotificationService _hubContext;

    public TriggerMonitorCheckController(IAsyncHandler<ManualCheckCommand, Result> handler ,INotificationService hubContext)
    {
        _handler = handler;
        _hubContext = hubContext;
    }

    [HttpPost("{id:guid}/run-now")]
    [EnableRateLimiting(RateLimitPolicies.ManualMonitorTrigger)]
    public async Task<IActionResult> RunNowAsync(Guid id, CancellationToken ct)
    {
        Result<MonitorListResult> result = await _handler.HandleAsync(new ManualCheckCommand(id), ct);

        if (result.IsSuccess)
        {
            await _hubContext.NotifyAsync(result.Value, ct);
        }


        return ToActionResult(result);
    }
}
