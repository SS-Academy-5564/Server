using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.DashboardWidgets.GetWidgets;

namespace Pulse.API.Features.DashboardWidgets.GetWidgets;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public class GetWidgetsController : PulseControllerBase
{
    public readonly IAsyncHandler<GetWidgetsQuery, Result<IReadOnlyList<GetWidgetsResult>>> _handler;

    public GetWidgetsController(
        IAsyncHandler<GetWidgetsQuery, Result<IReadOnlyList<GetWidgetsResult>>> handler)
    {
        _handler = handler;
    }

    [HttpGet("{dashboardTabId:guid}/widgets")]
    public async Task<IActionResult> GetWidgetsAsync(
        Guid dashboardTabId,
        CancellationToken ct)
    {
        Result<IReadOnlyList<GetWidgetsResult>> result =
           await _handler.HandleAsync(
               new GetWidgetsQuery(dashboardTabId),
               ct);

        return ToActionResult(result);
    }
}
