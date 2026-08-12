using Dapper;
using Pulse.DAL.Commands.DashboardWidgets.CreateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.DashboardWidgets;

public class WidgetCommands : IWidgetCommands
{
    private readonly IDbSessionAccessor _sessionAccessor;

    public WidgetCommands(IDbSessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }

    public async Task<Guid> CreateAsync(
       CreateWidgetInput input,
       CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        Guid id = Guid.NewGuid();

        const string sql =
            """
            INSERT INTO dbo.DashboardWidgets
            (
                Id,
                DashboardTabId,
                OrganizationId,
                MonitorId,
                Type,
                Title,
                Subtitle,
                Metric,
                TimeRange,
                Settings
            )
            OUTPUT INSERTED.Id
            VALUES
            (
                @Id,
                @DashboardTabId,
                @OrganizationId,
                @MonitorId,
                @Type,
                @Title,
                @Subtitle,
                @Metric,
                @TimeRange,
                @Settings
            );
            """;

        return await session.Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                sql,
                new
                {
                    Id = id,
                    input.DashboardTabId,
                    input.OrganizationId,
                    input.MonitorId,
                    input.Type,
                    input.Title,
                    input.Subtitle,
                    input.Metric,
                    input.TimeRange,
                    input.Settings
                },
                transaction: session.Transaction,
                cancellationToken: ct));
    }
}
