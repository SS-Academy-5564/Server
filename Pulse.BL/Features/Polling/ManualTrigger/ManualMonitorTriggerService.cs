using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.BackgroundTasks;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling.ManualTrigger;

public sealed class ManualMonitorTriggerService : IManualMonitorTriggerService
{
    public const string MonitorNotFoundErrorCode = "MonitorNotFound";

    private readonly IMonitorQueries _monitorQueries;
    private readonly IBackgroundTaskQueue _taskQueue;
    private readonly ILogger<ManualMonitorTriggerService> _logger;

    public ManualMonitorTriggerService(
        IMonitorQueries monitorQueries,
        IBackgroundTaskQueue taskQueue,
        ILogger<ManualMonitorTriggerService> logger)
    {
        _monitorQueries = monitorQueries;
        _taskQueue = taskQueue;
        _logger = logger;
    }

    public async Task<Result> TriggerAsync(Guid monitorId, CancellationToken ct)
    {
        MonitorPollingRecord? monitor = await _monitorQueries.GetByIdForPollingAsync(monitorId, ct);

        if (monitor is null)
        {
            return Result.Fail(new Error("Monitor not found.")
                .WithMetadata("Code", MonitorNotFoundErrorCode));
        }

        await _taskQueue.EnqueueAsync(async (sp, token) =>
        {
            IPollingService pollingService = sp.GetRequiredService<IPollingService>();
            await pollingService.ProcessMonitorAsync(monitor, token);
        });

        _logger.LogInformation("Manual check enqueued. MonitorId: {MonitorId}", monitorId);

        return Result.Ok();
    }
}