using FluentResults;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling;

public interface IPollingService
{
    Task<Result<List<UpdateMonitorAfterPollInput>>> ProcessDueMonitorsAsync(CancellationToken stoppingToken);
    Task<Result<UpdateMonitorAfterPollInput>> ProcessMonitorAsync(MonitorPollingRecord monitor, CancellationToken ct);
    Task<Result<UpdateMonitorAfterPollInput>> ProcessMonitorAsync(Guid monitorId, Guid organizationId, CancellationToken ct);
}
