using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors.GetMonitorById;

namespace Pulse.API.Features.Monitors.GetMonitorById;

[ApiController]
[Route("api/monitors")]
[Authorize]
public class GetMonitorByIdController : PulseControllerBase
{
    private readonly IAsyncHandler<GetMonitorByIdQuery, Result<MonitorResult>> _handler;

    public GetMonitorByIdController(IAsyncHandler<GetMonitorByIdQuery, Result<MonitorResult>> handler)
    {
        _handler = handler;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetMonitorByIdAsync([FromRoute] Guid id, CancellationToken ct = default)
    {
        GetMonitorByIdQuery query = new(id);

        Result<MonitorResult> result = await _handler.HandleAsync(query, ct);

        return ToActionResult(result);
    }
}
