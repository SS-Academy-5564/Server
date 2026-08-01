using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;

namespace Pulse.API.Features.Monitors.CreateMonitor;

/// <summary>
/// Controller for creating and managing monitors.
/// </summary>
[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class CreateMonitorController : PulseControllerBase
{
    private readonly IAsyncHandler<CreateMonitorCommand, Result<MonitorListResult>> _handler;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateMonitorController"/> class.
    /// </summary>
    /// <param name="handler">The handler for creating monitors.</param>
    public CreateMonitorController(IAsyncHandler<CreateMonitorCommand, Result<MonitorListResult>> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Creates a new monitor with the specified configuration.
    /// </summary>
    /// <param name="request">The monitor creation request.</param>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
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
