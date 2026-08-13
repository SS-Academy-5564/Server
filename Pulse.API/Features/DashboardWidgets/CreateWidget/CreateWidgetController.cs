using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.DashboardWidgets.CreateWidget;

namespace Pulse.API.Features.DashboardWidgets.CreateWidget;

[ApiController]
[Route("api/dashboard/widgets")]
[Authorize]
public class CreateWidgetController : PulseControllerBase
{
    private readonly IAsyncHandler<CreateWidgetCommand, Result<CreateWidgetResult>> _handler;

    public CreateWidgetController(
        IAsyncHandler<CreateWidgetCommand, Result<CreateWidgetResult>> handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWidgetAsync(
        [Validate] CreateWidgetRequest request,
        CancellationToken ct)
    {
        CreateWidgetCommand command = new(
            request.DashboardTabId,
            request.Type,
            request.Title,
            request.Subtitle,
            request.Metric,
            request.TimeRange,
            request.Settings,
            request.MonitorId
        );

        Result<CreateWidgetResult> result = await _handler.HandleAsync(command, ct);

        return ToActionResult(result);
    }
}
