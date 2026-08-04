using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Monitors;
using Pulse.BL.Features.Polling.ManualCheck.Queue;

namespace Pulse.BL.Features.Polling.ManualCheck;

/// <summary>
/// Background service that dequeues manually-triggered monitor checks and processes them.
/// </summary>
public sealed class ManualCheckQueueWorker : BackgroundService
{
    private readonly IManualCheckQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ManualCheckQueueWorker> _logger;
    public ManualCheckQueueWorker(
        IManualCheckQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ManualCheckQueueWorker> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Guid monitorId;

            try
            {
                monitorId = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IPollingService pollingService = scope.ServiceProvider.GetRequiredService<IPollingService>();
                var monitor = await pollingService.ProcessMonitorAsync(monitorId, stoppingToken);

                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                await notificationService.NotifyAsync(monitor.Value,stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual check failed. MonitorId: {MonitorId}", monitorId);
            }
        }
    }
}
