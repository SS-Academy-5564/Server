using System.Collections.Concurrent;
using FluentResults;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.Options;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Worker.Polling;

public sealed class PollerWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PollerWorker> _logger;
    private readonly PollingWorkerOptions _options;

    public PollerWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<PollerWorker> logger,
        IOptions<PollingWorkerOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_options.LoopIntervalSeconds));

        do
        {
            try
            {
                using IServiceScope scope = _scopeFactory.CreateScope();
                IPollingService pollingService = scope.ServiceProvider.GetRequiredService<IPollingService>();

                Result<IEnumerable<MonitorPollingRecord>> monitors = await pollingService.GetDueEnabledAsync(_options.BatchSize, stoppingToken);
                ConcurrentBag<MonitorPollResult> monitorPollResults = new();

                ParallelOptions options = new() { CancellationToken = stoppingToken, MaxDegreeOfParallelism = _options.MaxDegreeOfParallelism };

                await Parallel.ForEachAsync(monitors.Value, options, async (monitor, ct) =>
                {
                    Result<MonitorPollResult> monitorsResults = await pollingService.ProcessMonitorAsync(monitor, ct);
                    if (monitorsResults.IsSuccess)
                    {
                        monitorPollResults.Add(monitorsResults.Value);
                    }
                });

                if (monitors.IsSuccess)
                {
                    IBatchMonitorNotificationService notificationService = scope.ServiceProvider.GetRequiredService<IBatchMonitorNotificationService>();
                    await notificationService.NotifyAsync(monitorPollResults.ToList(), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (
                stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Poller Worker iteration failed.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
