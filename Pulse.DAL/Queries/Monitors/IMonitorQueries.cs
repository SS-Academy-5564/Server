using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Monitors;

/// <summary>
/// Defines query operations for retrieving monitors.
/// </summary>
public interface IMonitorQueries : IQueries
{
    /// <summary>
    /// Retrieves all monitor records, optionally filtered by status.
    /// </summary>
    /// <param name="status">The monitor status to filter by, or <c>null</c> to retrieve all monitors.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A list of monitor records matching the criteria.</returns>
    Task<IReadOnlyList<MonitorRecord>> GetAllAsync(MonitorStatus? status, CancellationToken ct);
}
