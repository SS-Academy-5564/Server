using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Monitors;

public class MonitorCommands : IMonitorCommands
{
    private readonly IDbSessionAccessor _sessionAccessor;

    public MonitorCommands(IDbSessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }

    public async Task UpdateAfterPollAsync(UpdateMonitorAfterPollInput input, CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

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
