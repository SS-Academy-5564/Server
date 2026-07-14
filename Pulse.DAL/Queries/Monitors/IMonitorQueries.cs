using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Monitors;

public interface IMonitorQueries : IQueries
{
    Task<IEnumerable<MonitorPollingRecord>> GetDueEnabledAsync(int max, CancellationToken ct = default);
    Task<IReadOnlyList<MonitorListRecord>> GetAllAsync(MonitorStatus? status, CancellationToken ct);
}
