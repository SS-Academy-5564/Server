using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Monitors;

public interface IMonitorQueries : IQueries
{
    Task<IEnumerable<MonitorRecord>> GetDueEnabledAsync(int max,CancellationToken ct =default);
}
