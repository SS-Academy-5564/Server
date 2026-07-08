using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Monitors;

public interface IMonitorQueries : IQueries
{
    Task<IReadOnlyList<MonitorRecord>> GetAllAsync(MonitorStatus? status, CancellationToken ct);
}
