using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Monitors.GetMonitorsLookup;

public class GetMonitorsLookupHandler : IAsyncQueryHandler<Result<IEnumerable<MonitorLookupResult>>>
{
    private readonly IMonitorQueries _monitorQueries;
    private readonly ICurrentUserService _currentUserService;

    public GetMonitorsLookupHandler(IMonitorQueries monitorQueries, ICurrentUserService currentUser)
    {
        _monitorQueries = monitorQueries;
        _currentUserService = currentUser;
    }

    public async Task<Result<IEnumerable<MonitorLookupResult>>> HandleAsync(CancellationToken ct)
    {
        Result<Guid> organizationIdResult = _currentUserService.RequireOrganizationId();

        if (organizationIdResult.IsFailed)
        {
            return organizationIdResult.ToResult();
        }

        IEnumerable<MonitorLookupRecord> monitorsRecords = await _monitorQueries.GetMonitorsLookupAsync(organizationIdResult.Value, ct);
        IEnumerable<MonitorLookupResult> monitorsResult = monitorsRecords.Select(m => new MonitorLookupResult(m.Name, m.Id));

        return monitorsResult.ToResult();
    }
}
