using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.DashboardWidgets.GetWidgets;

public class WidgetQueries : IWidgetQueries
{
    private readonly IDbSessionAccessor _sessionAccessor;

    public WidgetQueries(IDbSessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }

    public async Task<IReadOnlyList<WidgetQueryResult>> GetByTabIdAsync(
        Guid dashboardTabId,
        Guid organizationId,
        CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        const string sql =
        """
        SELECT
            Id,
            DashboardTabId,
            Type,
            Title,
            Subtitle,
            Metric,
            TimeRange,
            Settings,
            CAST(NULL AS DECIMAL(18,2)) AS Value
        FROM dbo.DashboardWidgets
        WHERE DashboardTabId = @DashboardTabId
          AND OrganizationId = @OrganizationId;
        """;

        IEnumerable<WidgetQueryResult> widgets =
            await session.Connection.QueryAsync<WidgetQueryResult>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        dashboardTabId,
                        organizationId
                    },
                    transaction: session.Transaction,
                    cancellationToken: ct));

        return widgets.ToList();
    }
}
