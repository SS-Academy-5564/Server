using FluentResults;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Polling.ManualCheck.Queue;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling.ManualCheck;

public sealed class ManualCheckHandler : IAsyncHandler<ManualCheckCommand, Result>
{
    private readonly IMonitorQueries _monitorQueries;
    private readonly IManualCheckQueue _queue;
    private readonly ILogger<ManualCheckHandler> _logger;

    public ManualCheckHandler(
        IMonitorQueries monitorQueries,
        IManualCheckQueue queue,
        ILogger<ManualCheckHandler> logger)
    {
        _monitorQueries = monitorQueries;
        _queue = queue;
        _logger = logger;
    }

    /// <summary>
    /// Tries adding the monitor into the queue
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <returns></returns>
    public async Task<Result> HandleAsync(ManualCheckCommand command, CancellationToken ct = default)
    {
        MonitorPollingRecord? monitor = await _monitorQueries.GetByIdForPollingAsync(command.MonitorId, ct);

        if (monitor is null)
        {
            return Result.Fail(new NotFoundError($"Monitor '{command.MonitorId}' was not found."));
        }

        if (!_queue.TryEnqueue(command.MonitorId))
        {
            _logger.LogWarning("Manual check queue is full. MonitorId: {MonitorId}", command.MonitorId);
            return Result.Fail(new TooManyRequestsError(
                "Too many manual checks are queued right now. Please try again shortly."));
        }

        _logger.LogInformation("Manual check enqueued. MonitorId: {MonitorId}", command.MonitorId);

        return Result.Ok();
    }
}
