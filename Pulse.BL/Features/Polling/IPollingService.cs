using FluentResults;
using Pulse.BL.Features.Polling.UpdateNotifier;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling;

public interface IPollingService
{
    Task<Result<List<MonitorUpdate>>> ProcessDueMonitorsAsync(CancellationToken stoppingToken);
    Task<Result<MonitorUpdate>> ProcessMonitorAsync(MonitorPollingRecord monitor, CancellationToken ct);
    Task<Result<MonitorUpdate>> ProcessMonitorAsync(Guid monitorId, CancellationToken ct = default);
}
