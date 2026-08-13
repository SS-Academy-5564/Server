using Dapper;
using Pulse.DAL.Commands.DashboardWidgets.CreateWidget;
using Pulse.DAL.Commands.DashboardWidgets.UpdateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.DashboardWidgets;

/// <inheritdoc cref="IWidgetCommands"/>
public class WidgetCommands : IWidgetCommands
{
    private readonly IDbSessionAccessor _sessionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="WidgetCommands"/> class.
    /// </summary>
    /// <param name="sessionAccessor">The session accessor.</param>
    public WidgetCommands(IDbSessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public async Task<bool> UpdateAsync(
        UpdateWidgetInput input,
        CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        const string sql =
            """
            UPDATE dbo.DashboardWidgets
            SET
                Type = @Type,
                Title = @Title,
                Subtitle = @Subtitle,
                Metric = @Metric,
                TimeRange = @TimeRange,
                Settings = @Settings
            WHERE Id = @Id
              AND OrganizationId = @OrganizationId;
            """;

        int affectedRows = await session.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                input,
                transaction: session.Transaction,
                cancellationToken: ct));

        return affectedRows > 0;
    }
}
