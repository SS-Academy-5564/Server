using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;

namespace Pulse.API.Features.Monitors.CreateMonitor;

[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class CreateMonitorController : PulseControllerBase
{
    private readonly IAsyncHandler<CreateMonitorCommand, Result<MonitorListResult>> _handler;

    public CreateMonitorController(IAsyncHandler<CreateMonitorCommand, Result<MonitorListResult>> handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateMonitorAsync([Validate] CreateMonitorRequest request, CancellationToken ct)
    {
        CreateMonitorCommand command = new(
            request.Name,
            request.Url,
            request.HttpMethod,
            request.ResultPath,
            request.PollingIntervalSeconds,
            request.PollingTimeoutSeconds);

        Result<MonitorListResult> result = await _handler.HandleAsync(command, ct);
        return ToActionResult(result);
    }
}
