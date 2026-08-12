using Pulse.DAL.Common.Pagination;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.Monitors;

/// <summary>
/// Defines query operations for retrieving monitors.
/// </summary>
public interface IMonitorQueries : IQueries
{
    /// <summary>
    /// Retrieves one page of monitor records for a specific organization, optionally filtered by status and search string.
    /// </summary>
    /// <param name="organizationId">The organization ID to filter monitors by.</param>
    /// <param name="status">The monitor status to filter by, or <c>null</c> to retrieve all monitors.</param>
    /// <param name="pageNumber">The one-based page number.</param>
    /// <param name="pageSize">The maximum number of records to return.</param>
    /// <param name="searchString">Optional search string to filter monitors by name.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The requested monitor records and the total number of matching records.</returns>
    Task<PagedRecords<MonitorListRecord>> GetAllAsync(
        Guid organizationId,
        MonitorStatus? status,
        int pageNumber,
        int pageSize,
        string? searchString,
        CancellationToken ct);

    /// <summary>
    /// Retrieves enabled monitors that are due for polling.
    /// </summary>
    /// <param name="max">The maximum number of monitors to return.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A collection of monitors ready for polling.</returns>
    Task<IEnumerable<MonitorPollingRecord>> GetDueEnabledAsync(int max, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the monitor by its id.
    /// </summary>
    /// <param name="id">The monitor id to search by.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="MonitorRecord"/> matching the given <paramref name="id"/>,
    /// or <see langword="null"/> if no monitor with that identifier exists.
    /// </returns>
    Task<MonitorRecord?> GetByIdAsync(Guid id, CancellationToken ct);

    Task<MonitorPollingRecord?> GetByIdForPollingAsync(Guid id, CancellationToken ct);

    Task<IEnumerable<MonitorLookupRecord>> GetMonitorsLookupAsync(Guid organizationId, CancellationToken ct);
}
