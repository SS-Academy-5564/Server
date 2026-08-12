using FluentResults;
using Microsoft.Extensions.Logging;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.DashboardWidgets.GetWidgets;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.DashboardWidgets.GetWidgets;

public class GetWidgetsHandler
    : IAsyncHandler<GetWidgetsQuery, Result<IReadOnlyList<GetWidgetsResult>>>
{
    private readonly IWidgetQueries _widgetQueries;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMonitorQueries  _monitorQueries;

    public GetWidgetsHandler(
        IWidgetQueries widgetQueries,
        ICurrentUserService currentUserService,
        IUnitOfWorkFactory unitOfWorkFactory,
        IMonitorQueries monitorQueries)
    {
        _widgetQueries = widgetQueries;
        _currentUserService = currentUserService;
        _unitOfWorkFactory = unitOfWorkFactory;
        _monitorQueries = monitorQueries;
    }

    public async Task<Result<IReadOnlyList<GetWidgetsResult>>> HandleAsync(
        GetWidgetsQuery query,
        CancellationToken ct = default)
    {
        Result<Guid> organizationResult =
            _currentUserService.RequireOrganizationId();

        if (organizationResult.IsFailed)
        {
            return Result.Fail(organizationResult.Errors);
        }

        await using IUnitOfWork uow = await _unitOfWorkFactory.CreateAsync(ct: ct);

        IReadOnlyList<WidgetQueryResult> widgets =
            await _widgetQueries.GetByTabIdAsync(
                query.DashboardTabId,
                organizationResult.Value,
                ct);

        var monitorIds = widgets.Select(w => w.MonitorId);
        ILookup<Guid, string> statsLookup = await _monitorQueries.GetMonitorsStatisticsAsync(monitorIds, ct);

        var results = widgets.Select(x => new GetWidgetsResult(
            x.Id,
            x.DashboardTabId,
            x.Type,
            x.Title,
            x.Subtitle,
            x.Metric,
            x.TimeRange,
            x.Settings,
            Value: statsLookup[x.MonitorId]
        )).ToList();

        return Result.Ok<IReadOnlyList<GetWidgetsResult>>(results);
    }
}
