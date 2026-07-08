using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Monitors;

public class MonitorCommands : IMonitorCommands
{
    public async Task UpdateAfterPollAsync(UpdateMonitorAfterPollInput input, IUnitOfWork uow, CancellationToken ct)
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

        await uow.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                input,
                transaction: uow.Transaction,
                cancellationToken: ct));
    }
}
