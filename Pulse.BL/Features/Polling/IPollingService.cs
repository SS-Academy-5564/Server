using FluentResults;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling;

public interface IPollingService
{
    Task<Result> ProcessDueMonitorsAsync(CancellationToken stoppingToken);
    Task<Result> ProcessMonitorAsync(MonitorPollingRecord monitor, CancellationToken ct);
}
