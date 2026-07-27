using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;
using Pulse.BL.Features.Monitors.UpdateMonitor;

namespace Pulse.API.Features.Monitors.UpdateMonitor;

[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class UpdateMonitorController : PulseControllerBase
{
    private readonly IAsyncHandler<UpdateMonitorCommand, Result<MonitorListResult>> _handler;

    public UpdateMonitorController(IAsyncHandler<UpdateMonitorCommand, Result<MonitorListResult>> handler)
    {
        _handler = handler;
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMonitorAsync([FromRoute] Guid id, [Validate] UpdateMonitorRequest request, CancellationToken ct = default)
    {
        UpdateMonitorCommand command = new(
            id,
            request.Name,
            request.Url,
            request.HttpMethod,
            request.ResultPath,
            request.Status,
            request.PollingIntervalSeconds,
            request.PollingTimeoutSeconds);

        Result<MonitorListResult> result = await _handler.HandleAsync(command, ct);
        return ToActionResult(result);
    }
}
