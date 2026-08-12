using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors.UpdateMonitorStatus;

namespace Pulse.API.Features.Monitors.UpdateMonitorStatus;

[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class UpdateMonitorStatusController : PulseControllerBase
{
    private readonly IAsyncHandler<UpdateMonitorStatusCommand, Result> _handler;

    public UpdateMonitorStatusController(
        IAsyncHandler<UpdateMonitorStatusCommand, Result> handler)
    {
        _handler = handler;
    }

    [HttpPatch("{monitorId:guid}/status")]
    public async Task<IActionResult> UpdateMonitorStatusAsync(
        Guid monitorId,
        [Validate] UpdateMonitorStatusRequest request,
        CancellationToken ct)
    {
        Result result = await _handler.HandleAsync(
            new UpdateMonitorStatusCommand(monitorId, request.Status),
            ct);

        return ToActionResult(result);
    }
}
