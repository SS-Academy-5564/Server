using FluentResults;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Polling.ManualCheck.Queue;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling.ManualCheck;

public sealed class ManualCheckHandler : IAsyncHandler<ManualCheckCommand, Result>
{
    private readonly IMonitorQueries _monitorQueries;
    private readonly IManualCheckQueue _queue;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ManualCheckHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualCheckHandler"/> class.
    /// </summary>
    /// <param name="monitorQueries">The queries service for retrieving monitor data.</param>
    /// <param name="queue">The bounded manual check queue.</param>
    /// <param name="currentUserService">The current user service.</param>
    /// <param name="logger">The logger for this handler.</param>
    public ManualCheckHandler(
        IMonitorQueries monitorQueries,
        IManualCheckQueue queue,
        ICurrentUserService currentUserService,
        ILogger<ManualCheckHandler> logger)
    {
        _monitorQueries = monitorQueries;
        _queue = queue;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Attempts to add a manual monitor check to the queue.
    /// </summary>
    /// <param name="command">The manual check command containing the monitor and organization identifiers.</param>
    /// <param name="ct">The cancellation token for the operation.</param>
    /// <returns>A result indicating whether the monitor check was queued.</returns>
    /// <exception cref="OperationCanceledException">The operation is canceled while retrieving the monitor.</exception>
    public async Task<Result> HandleAsync(ManualCheckCommand command, CancellationToken ct = default)
    {
        MonitorPollingRecord? monitor = await _monitorQueries.GetByIdForPollingAsync(
            command.MonitorId,
            ct);

        if (monitor is null || monitor.OrganizationId != command.OrganizationId)
        {
            return Result.Fail(new NotFoundError($"Monitor '{command.MonitorId}' was not found."));
        }

        if (!_queue.TryEnqueue(command))
        {
            _logger.LogWarning("Manual check queue is full. MonitorId: {MonitorId}", command.MonitorId);
            return Result.Fail(new TooManyRequestsError(
                "Too many manual checks are queued right now. Please try again shortly."));
        }

        _logger.LogInformation("Manual check enqueued. MonitorId: {MonitorId}", command.MonitorId);

        return Result.Ok();
    }
}
