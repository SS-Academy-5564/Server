using Microsoft.Extensions.Logging;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling.ManualTrigger;

public sealed class ManualCheckExecutor : IManualCheckExecutor
{
    private readonly IMonitorQueries _monitorQueries;
    private readonly IPollingService _pollingService;
    private readonly ILogger<ManualCheckExecutor> _logger;

    public ManualCheckExecutor(
        IMonitorQueries monitorQueries,
        IPollingService pollingService,
        ILogger<ManualCheckExecutor> logger)
    {
        _monitorQueries = monitorQueries;
        _pollingService = pollingService;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid monitorId, CancellationToken ct)
    {
        MonitorPollingRecord? monitor = await _monitorQueries.GetByIdForPollingAsync(monitorId, ct);

        if (monitor is null)
        {
            _logger.LogWarning(
                "Skipping manual check: monitor is no longer eligible. MonitorId: {MonitorId}",
                monitorId);
            return;
        }

        await _pollingService.ProcessMonitorAsync(monitor, ct);
    }
}
