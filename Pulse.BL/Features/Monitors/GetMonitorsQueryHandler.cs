using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Monitors;

public class GetMonitorsQueryHandler : IAsyncHandler<GetMonitorsQuery, Result<IReadOnlyList<MonitorResult>>>
{
    private readonly IMonitorQueries _monitorQueries;

    public GetMonitorsQueryHandler(IMonitorQueries monitorQueries)
    {
        _monitorQueries = monitorQueries;
    }

    /// <summary>
    /// Handles the query to retrieve monitor results.
    /// </summary>
    /// <param name="query">The query parameters, including the optional status filter.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A list of monitor results or failure details.</returns>
    /// <remarks>
    /// If <see cref="GetMonitorsQuery.Status"/> is <see langword="null"/>,
    /// all monitors are returned. Otherwise, only monitors with the specified
    /// status are retrieved.
    /// </remarks>
    public async Task<Result<IReadOnlyList<MonitorResult>>> HandleAsync(GetMonitorsQuery query, CancellationToken ct = default)
    {
        DAL.Queries.Monitors.MonitorStatus? dalStatus = query.Status is null
            ? null
            : (DAL.Queries.Monitors.MonitorStatus)query.Status.Value;

        IReadOnlyList<MonitorRecord> records = await _monitorQueries.GetAllAsync(dalStatus, ct);

        IReadOnlyList<MonitorResult> results = records
            .Select(r => new MonitorResult(r.Id, r.Name, r.Url, r.CurrentValue, r.LastCheckedAt, (MonitorStatus)r.Status, r.Interval))
            .ToList()
            .AsReadOnly();

        return Result.Ok(results);
    }
}
