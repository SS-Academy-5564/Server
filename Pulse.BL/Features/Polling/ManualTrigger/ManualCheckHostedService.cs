using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pulse.BL.Features.Polling.ManualTrigger;

public sealed class ManualCheckHostedService : BackgroundService
{
    private readonly IManualCheckQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ManualCheckHostedService> _logger;

    public ManualCheckHostedService(
        IManualCheckQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<ManualCheckHostedService> logger)
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
                IManualCheckExecutor executor = scope.ServiceProvider.GetRequiredService<IManualCheckExecutor>();
                await executor.ExecuteAsync(monitorId, stoppingToken);
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
