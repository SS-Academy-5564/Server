using Pulse.DAL.Common.Pagination;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Monitors;

/// <summary>
/// Defines query operations for retrieving monitors.
/// </summary>
public interface IMonitorQueries : IQueries
{
    /// <summary>
    /// Retrieves one page of monitor records, optionally filtered by status.
    /// </summary>
    /// <param name="status">The monitor status to filter by, or <c>null</c> to retrieve all monitors.</param>
    /// <param name="pageNumber">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of records to return.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The requested monitor records and the total number of matching records.</returns>
    Task<PagedRecords<MonitorListRecord>> GetAllAsync(
        MonitorStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken ct);

    Task<IEnumerable<MonitorPollingRecord>> GetDueEnabledAsync(int max, CancellationToken ct = default);
}
