using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Polling;

namespace Pulse.Worker.Polling;

public class PollerWorker : BackgroundService
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

                await pollingService.ProcessDueMonitorsAsync(stoppingToken);
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL error occurred during polling");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
