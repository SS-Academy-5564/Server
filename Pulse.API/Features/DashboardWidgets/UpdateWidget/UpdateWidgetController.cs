using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.DashboardWidgets.UpdateWidget;

namespace Pulse.API.Features.DashboardWidgets.UpdateWidget;

/// <summary>
/// Provides endpoints for updating dashboard widgets.
/// </summary>
[ApiController]
[Route("api/dashboard/widgets")]
[Authorize]
public class UpdateWidgetController : PulseControllerBase
{
    private readonly IAsyncHandler<UpdateWidgetCommand, Result> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateWidgetController"/> class.
    /// </summary>
    /// <param name="handler">The update widget handler.</param>
    public UpdateWidgetController(
        IAsyncHandler<UpdateWidgetCommand, Result> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Updates the configuration of an existing widget.
    /// </summary>
    /// <param name="widgetId">The identifier of the widget to update.</param>
    /// <param name="request">The updated widget configuration.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>An HTTP response indicating the outcome of the update.</returns>
    [HttpPut("{widgetId:guid}")]
    public async Task<IActionResult> UpdateWidgetAsync(
        Guid widgetId,
        [Validate] UpdateWidgetRequest request,
        CancellationToken ct)
    {
        UpdateWidgetCommand command = new(
            widgetId,
            request.Type,
            request.Title,
            request.Subtitle,
            request.Metric,
            request.TimeRange,
            request.Settings
        );

        Result result = await _handler.HandleAsync(command, ct);

        return ToActionResult(result);
    }
}
