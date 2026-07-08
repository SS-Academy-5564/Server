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

    public async Task<Result<IReadOnlyList<MonitorResult>>> HandleAsync(GetMonitorsQuery query, CancellationToken ct = default)
    {
        IReadOnlyList<MonitorRecord> records = await _monitorQueries.GetAllAsync(query.Status, ct);

        IReadOnlyList<MonitorResult> results = records
            .Select(r => new MonitorResult(r.Id, r.Name, r.Url, r.CurrentValue, r.LastCheckedAt, r.Status, r.Interval))
            .ToList()
            .AsReadOnly();

        return Result.Ok(results);
    }
}
