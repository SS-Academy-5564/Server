using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Monitors;

public class MonitorCommands : IMonitorCommands
{
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
