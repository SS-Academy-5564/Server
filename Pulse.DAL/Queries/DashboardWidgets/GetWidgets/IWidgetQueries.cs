using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.DashboardWidgets.GetWidgets;

public interface IWidgetQueries : IQueries
{
    /// <summary>
    /// Retrieves all widgets for the specified dashboard tab and organization.
    /// </summary>
    /// <param name="dashboardTabId">The identifier of the dashboard tab.</param>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>A collection of widgets for the specified dashboard tab.</returns>
    Task<IReadOnlyList<WidgetQueryResult>> GetByTabIdAsync(
        Guid dashboardTabId,
        Guid organizationId,
        CancellationToken ct);
}
