using Dapper;
using Pulse.DAL.Commands.DashboardWidgets.CreateWidget;
using Pulse.DAL.Commands.DashboardWidgets.UpdateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.DashboardWidgets;

/// <summary>
/// Persists dashboard widgets through Dapper commands.
/// </summary>
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

    /// <summary>
    /// Creates a widget within the current organization.
    /// </summary>
    /// <param name="input">The widget configuration to persist.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The identifier of the created widget.</returns>
    /// <exception cref="InvalidOperationException">There is no active unit of work.</exception>
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

    /// <summary>
    /// Updates a widget within the current organization.
    /// </summary>
    /// <param name="input">The widget configuration to persist.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns><c>true</c> when a matching widget was updated; otherwise <c>false</c>.</returns>
    /// <exception cref="InvalidOperationException">There is no active unit of work.</exception>
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
