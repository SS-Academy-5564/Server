using FluentResults;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling;

public interface IPollingService
{
    Task<Result<List<MonitorPollResult>>> ProcessDueMonitorsAsync(CancellationToken stoppingToken);
    Task<Result<MonitorPollResult>> ProcessMonitorAsync(MonitorPollingRecord monitor, CancellationToken ct);
    Task<Result<MonitorPollResult>> ProcessMonitorAsync(Guid monitorId, Guid organizationId, CancellationToken ct);
}
