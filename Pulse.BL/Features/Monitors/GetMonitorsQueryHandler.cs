using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Pagination;
using Pulse.DAL.Common.Pagination;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Monitors;

public class GetMonitorsQueryHandler : IAsyncHandler<GetMonitorsQuery, Result<PagedResult<MonitorListResult>>>
{
    private readonly IMonitorQueries _monitorQueries;

    public GetMonitorsQueryHandler(IMonitorQueries monitorQueries)
    {
        _monitorQueries = monitorQueries;
    }

    /// <summary>
    /// Retrieves a filtered and paginated list of monitor results.
    /// </summary>
    /// <param name="query">The optional status filter, page number, and page size.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>The monitor results and pagination metadata.</returns>
    /// <remarks>
    /// A missing page number defaults to <see cref="PaginationDefaults.PageNumber"/>, and a missing page size
    /// defaults to <see cref="PaginationDefaults.PageSize"/>. A missing status returns monitors of every status.
    /// </remarks>
    public async Task<Result<PagedResult<MonitorListResult>>> HandleAsync(
        GetMonitorsQuery query,
        CancellationToken ct = default)
    {
        DAL.Queries.Monitors.MonitorStatus? dalStatus = query.Status is null
            ? null
            : (DAL.Queries.Monitors.MonitorStatus)query.Status.Value;

        int pageNumber = query.PageNumber ?? PaginationDefaults.PageNumber;
        int pageSize = query.PageSize ?? PaginationDefaults.PageSize;

        PagedRecords<MonitorListRecord> records = await _monitorQueries.GetAllAsync(dalStatus, pageNumber, pageSize, ct);

        IReadOnlyList<MonitorListResult> results = records.Items
            .Select(r => new MonitorListResult(r.Id, r.Name, r.Url, r.CurrentValue, r.LastCheckedAt, (MonitorStatus)r.Status, r.Interval))
            .ToList()
            .AsReadOnly();

        return Result.Ok(new PagedResult<MonitorListResult>(results, pageNumber, pageSize, records.TotalCount));
    }
}
