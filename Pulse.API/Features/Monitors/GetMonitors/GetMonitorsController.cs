using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;

namespace Pulse.API.Features.Monitors.GetMonitors;

[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class GetMonitorsController : PulseControllerBase
{
    private readonly IAsyncHandler<GetMonitorsQuery, Result<IReadOnlyList<MonitorListResult>>> _handler;

    public GetMonitorsController(IAsyncHandler<GetMonitorsQuery, Result<IReadOnlyList<MonitorListResult>>> handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> GetMonitorsAsync([FromQuery][Validate] GetMonitorsRequest request, CancellationToken ct)
    {
        Result<IReadOnlyList<MonitorListResult>> result = await _handler.HandleAsync(new GetMonitorsQuery(request.Status, request.PageNumber, request.PageSize), ct);
        return ToActionResult(result);
    }
}
