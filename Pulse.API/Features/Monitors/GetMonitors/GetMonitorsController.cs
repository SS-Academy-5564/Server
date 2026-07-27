using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Pagination;
using Pulse.BL.Features.Monitors;

namespace Pulse.API.Features.Monitors.GetMonitors;

[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class GetMonitorsController : PulseControllerBase
{
    private readonly IAsyncHandler<GetMonitorsQuery, Result<PagedResult<MonitorListResult>>> _handler;

    public GetMonitorsController(IAsyncHandler<GetMonitorsQuery, Result<PagedResult<MonitorListResult>>> handler)
    {
        _handler = handler;
    }

    [HttpGet]
    public async Task<IActionResult> GetMonitorsAsync([FromQuery][Validate] GetMonitorsRequest request, CancellationToken ct)
    {
        Result<PagedResult<MonitorListResult>> result = await _handler.HandleAsync(
            new GetMonitorsQuery(request.Status, request.PageNumber, request.PageSize),
            ct);

        return ToPagedActionResult(result);
    }
}
