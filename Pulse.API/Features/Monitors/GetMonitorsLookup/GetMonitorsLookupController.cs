using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors.GetMonitorsLookup;

namespace Pulse.API.Features.Monitors.GetMonitorsLookup;

[ApiController]
[Route("api/monitors")]
[Authorize]
public sealed class GetMonitorsLookupController : PulseControllerBase
{
    private readonly IAsyncQueryHandler<Result<IEnumerable<MonitorLookupResult>>> _handler;

    public GetMonitorsLookupController(IAsyncQueryHandler<Result<IEnumerable<MonitorLookupResult>>> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Gets minimal monitor lookup items for select dropdowns.
    /// </summary>
    /// <param name="ct">A cancellation token to observe while waiting for the operation to complete.</param>
    /// <returns>An <see cref="IActionResult"/> representing the result of the operation.</returns>
    [HttpGet("lookup")]
    public async Task<IActionResult> CreateMonitorAsync(CancellationToken ct)
    {
        Result<IEnumerable<MonitorLookupResult>> result = await _handler.HandleAsync(ct);

        return ToActionResult(result);
    }
}
