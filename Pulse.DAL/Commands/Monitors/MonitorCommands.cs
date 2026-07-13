using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Monitors;

/// <inheritdoc cref="IMonitorCommands"/>
public class MonitorCommands : IMonitorCommands
{
    ///<inheritdoc/>
    public async Task UpdateAfterPollAsync(
        UpdateMonitorAfterPollInput input,
        IDbSession session,
        CancellationToken ct)
    {
        const string sql =
            """
            UPDATE dbo.Monitors
            SET
                CurrentValue = COALESCE(@CurrentValue, CurrentValue),
                LastCheckedAt = @LastCheckedAt,
                NextExecutionAt = @NextExecutionAt
            WHERE Id = @MonitorId;
            """;

        await session.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                input,
                transaction: session.Transaction,
                cancellationToken: ct));
    }
}
