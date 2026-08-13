using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.DashboardWidgets.GetWidgets;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.DashboardWidgets.GetWidgets;

/// <summary>
/// Handles the query to retrieve dashboard widgets for a specified tab.
/// </summary>
public class GetWidgetsHandler
    : IAsyncHandler<GetWidgetsQuery, Result<IReadOnlyList<GetWidgetsResult>>>
{
    private readonly IWidgetQueries _widgetQueries;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;
    private readonly IMonitorQueries _monitorQueries;

    /// <summary>
    /// Initializes a new instance of the <see cref="GetWidgetsHandler"/> class.
    /// </summary>
    /// <param name="widgetQueries">The widget queries repository.</param>
    /// <param name="currentUserService">The service providing current user context.</param>
    /// <param name="unitOfWorkFactory">The unit of work factory.</param>
    /// <param name="monitorQueries">The monitor queries repository.</param>
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

    /// <summary>
    /// Executes the widget retrieval operation asynchronously.
    /// </summary>
    /// <param name="query">The query containing the dashboard tab ID.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A result containing the list of widgets if successful; otherwise, an application error.</returns>
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

        IEnumerable<MonitorMetricRecord> monitorMetrics = widgets
            .Select(w => new MonitorMetricRecord(w.MonitorId, ParseMetric(w.Metric), w.TimeRange))
            .Distinct();

        ILookup<MonitorMetricRecord, decimal> stats =
            await _monitorQueries.GetMonitorsStatisticsAsync(monitorMetrics, ct);

        var results = widgets.Select(x => new GetWidgetsResult(
            x.Id,
            x.DashboardTabId,
            x.Type,
            x.Title,
            x.Subtitle,
            x.Metric,
            x.TimeRange,
            x.Settings,
            Value: stats[new MonitorMetricRecord(x.MonitorId, ParseMetric(x.Metric), x.TimeRange)]
        )).ToList();

        return Result.Ok<IReadOnlyList<GetWidgetsResult>>(results);
    }

    private static MetricType ParseMetric(string metric)
        => Enum.TryParse(metric, ignoreCase: true, out MetricType parsed)
            ? parsed
            : MetricType.ResponseTime;
}
