using FluentResults;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Polling.ManualTrigger.Queue;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling.ManualTrigger;

public sealed class ManualMonitorTriggerService : IManualMonitorTriggerService
{
    private readonly IMonitorQueries _monitorQueries;
    private readonly IManualCheckQueue _queue;
    private readonly ILogger<ManualMonitorTriggerService> _logger;

    public ManualMonitorTriggerService(
        IMonitorQueries monitorQueries,
        IManualCheckQueue queue,
        ILogger<ManualMonitorTriggerService> logger)
    {
        _monitorQueries = monitorQueries;
        _queue = queue;
        _logger = logger;
    }

    public async Task<Result> TriggerAsync(Guid monitorId, CancellationToken ct)
    {
        MonitorPollingRecord? monitor = await _monitorQueries.GetByIdForPollingAsync(monitorId, ct);

        if (monitor is null)
        {
            return Result.Fail(new NotFoundError($"Monitor '{monitorId}' was not found."));
        }

        if (!_queue.TryEnqueue(monitorId))
        {
            _logger.LogWarning("Manual check queue is full. MonitorId: {MonitorId}", monitorId);
            return Result.Fail(new TooManyRequestsError("Too many manual checks are queued right now. Please try again shortly."));
        }

        _logger.LogInformation("Manual check enqueued. MonitorId: {MonitorId}", monitorId);
        return Result.Ok();
    }
}
