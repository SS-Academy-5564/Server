using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Monitors.GetMonitorsLookup;

/// <summary>
/// Handles the query to retrieve a lookup list of monitors for the current organization.
/// </summary>
public class GetMonitorsLookupHandler : IAsyncQueryHandler<Result<IEnumerable<MonitorLookupResult>>>
{
    private readonly IMonitorQueries _monitorQueries;
    private readonly ICurrentUserService _currentUserService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetMonitorsLookupHandler"/> class.
    /// </summary>
    /// <param name="monitorQueries">The queries used to retrieve monitor data.</param>
    /// <param name="currentUser">The service used to resolve current user and organization information.</param>
    public GetMonitorsLookupHandler(IMonitorQueries monitorQueries, ICurrentUserService currentUser)
    {
        _monitorQueries = monitorQueries;
        _currentUserService = currentUser;
    }

    /// <summary>
    /// Handles retrieving minimal monitor lookup results for the current organization.
    /// </summary>
    /// <param name="ct">A token to cancel the operation.</param>
    /// <returns>A result containing the collection of monitor lookup results or failure details.</returns>
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
