using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Pulse.BL.Features.Polling.ManualTrigger.Queue;

namespace Pulse.BL.Features.Polling.ManualTrigger.Execution;

/// <summary>
/// A background service that continuously dequeues monitor IDs from the manual check queue and executes the corresponding checks.
/// </summary>
public sealed class ManualCheckHostedService : BackgroundService
{
    private readonly IManualCheckQueue _queue;
    private readonly IScopedManualCheckRunner _runner;
    private readonly ILogger<ManualCheckHostedService> _logger;

    public ManualCheckHostedService(
        IManualCheckQueue queue,
        IScopedManualCheckRunner runner,
        ILogger<ManualCheckHostedService> logger)
    {
        _queue = queue;
        _runner = runner;
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
                await _runner.RunAsync(monitorId, stoppingToken);
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
