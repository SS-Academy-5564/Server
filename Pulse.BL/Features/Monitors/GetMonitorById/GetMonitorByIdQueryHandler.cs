using FluentResults;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Monitors.GetMonitorById;

public class GetMonitorByIdQueryHandler : IAsyncHandler<GetMonitorByIdQuery, Result<MonitorResult>>
{
    private readonly IMonitorQueries _monitorQueries;

    public GetMonitorByIdQueryHandler(IMonitorQueries monitorQueries)
    {
        _monitorQueries = monitorQueries;
    }

    /// <summary>
    /// Handles the query to retrieve monitor record by id.
    /// </summary>
    /// <param name="query">The query parameter that contains id of the monitor.</param>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A monitor record or failure details.</returns>
    public async Task<Result<MonitorResult>> HandleAsync(GetMonitorByIdQuery query, CancellationToken ct)
    {
        MonitorRecord? existingMonitor = await _monitorQueries.GetByIdAsync(query.Id, ct);

        if (existingMonitor is null)
        {
            return Result.Fail(new NotFoundError("Monitor with this Id does not exist."));
        }

        MonitorResult result = new(
            existingMonitor.Id,
            existingMonitor.OrganizationId,
            existingMonitor.Name,
            existingMonitor.Url,
            existingMonitor.HttpMethod,
            existingMonitor.ResultPath,
            existingMonitor.CurrentValue,
            (MonitorStatus)existingMonitor.Status,
            existingMonitor.PollingIntervalSeconds,
            existingMonitor.PollingTimeoutSeconds,
            existingMonitor.LastCheckedAt,
            existingMonitor.NextExecutionAt,
            existingMonitor.CreatedAt,
            existingMonitor.LastModifiedAt);

        return Result.Ok(result);
    }
}
