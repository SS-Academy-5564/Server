using FluentResults;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling.ManualCheck.Queue;
using Pulse.DAL.Commands.Monitors;

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
            ManualCheckJob job;

            try
            {
                job = await _queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IPollingService pollingService = scope.ServiceProvider.GetRequiredService<IPollingService>();
                Result<UpdateMonitorAfterPollInput> monitor = await pollingService.ProcessMonitorAsync(
                    job.MonitorId,
                    job.OrganizationId,
                    stoppingToken);

                if (monitor.IsSuccess)
                {
                    INotificationService notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                    await notificationService.NotifyAsync(job.OrganizationId, monitor.Value, stoppingToken);
                }
                else
                {
                    _logger.LogWarning(
                        "Manual check did not complete successfully. MonitorId: {MonitorId}",
                        job.MonitorId);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual check failed. MonitorId: {MonitorId}", job.MonitorId);
            }
        }
    }
}
