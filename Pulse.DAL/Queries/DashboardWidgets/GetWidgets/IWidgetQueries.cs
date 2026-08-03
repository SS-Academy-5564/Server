using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.DashboardWidgets.GetWidgets;

public interface IWidgetQueries : IQueries
{
    Task<IReadOnlyList<WidgetQueryResult>> GetByTabIdAsync(
        Guid dashboardTabId,
        Guid organizationId,
        CancellationToken ct);
}
