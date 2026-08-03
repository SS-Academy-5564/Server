using FluentResults;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.DashboardWidgets.GetWidgets;

namespace Pulse.BL.Features.DashboardWidgets.GetWidgets;

public class GetWidgetsHandler
    : IAsyncHandler<GetWidgetsQuery, Result<IReadOnlyList<GetWidgetsResult>>>
{
    private readonly IWidgetQueries _widgetQueries;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public GetWidgetsHandler(
        IWidgetQueries widgetQueries,
        ICurrentUserService currentUserService,
        IUnitOfWorkFactory unitOfWorkFactory)
    {
        _widgetQueries = widgetQueries;
        _currentUserService = currentUserService;
        _unitOfWorkFactory = unitOfWorkFactory;
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

        await using IUnitOfWork uow =
            await _unitOfWorkFactory.CreateAsync(ct: ct);

        IReadOnlyList<WidgetQueryResult> widgets =
            await _widgetQueries.GetByTabIdAsync(
                query.DashboardTabId,
                organizationResult.Value,
                ct);

        return Result.Ok<IReadOnlyList<GetWidgetsResult>>(
            widgets.Select(x => new GetWidgetsResult(
                x.Id,
                x.DashboardTabId,
                x.Type,
                x.Metric,
                x.TimeRange,
                x.Settings
        )).ToList());
    }
}
