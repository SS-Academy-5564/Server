using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Monitors;

public class GetMonitorsQueryHandler : IAsyncHandler<GetMonitorsQuery, Result<IReadOnlyList<MonitorListResult>>>
{
    private readonly IMonitorQueries _monitorQueries;

    public GetMonitorsQueryHandler(IMonitorQueries monitorQueries)
    {
        _monitorQueries = monitorQueries;
    }

    public async Task<Result<IReadOnlyList<MonitorListResult>>> HandleAsync(GetMonitorsQuery query, CancellationToken ct = default)
    {
        DAL.Queries.Monitors.MonitorStatus? dalStatus = query.Status is null
            ? null
            : (DAL.Queries.Monitors.MonitorStatus)query.Status.Value;

        IReadOnlyList<MonitorListRecord> records = await _monitorQueries.GetAllAsync(dalStatus, ct);

        IReadOnlyList<MonitorListResult> results = records
            .Select(r => new MonitorListResult(r.Id, r.Name, r.Url, r.CurrentValue, r.LastCheckedAt, (MonitorStatus)r.Status, r.Interval))
            .ToList()
            .AsReadOnly();

        return Result.Ok(results);
    }
}
