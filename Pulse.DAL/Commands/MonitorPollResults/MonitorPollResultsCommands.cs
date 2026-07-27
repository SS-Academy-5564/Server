using Dapper;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.MonitorPollResults;

public class MonitorPollResultsCommands : IMonitorPollResultsCommands
{
    public async Task CreateAsync(CreateMonitorPollResultsInput monitorPollResultsInput, IDbSession session, CancellationToken ct)
    {
        const string sql =
            """
            INSERT INTO dbo.MonitorPollResults
                (Value, CheckedAt, IsSuccess, ResponseTimeMs, StatusCode, MonitorId, RequestStatusId)
            VALUES
                (
                    @Value,
                    @CheckedAt,
                    @IsSuccess,
                    @ResponseTimeMs,
                    @StatusCode,
                    @MonitorId,
                    (SELECT Id FROM dbo.RequestStatus WHERE Status = @RequestStatus)
                );
            """;

        await session.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                monitorPollResultsInput,
                transaction: session.Transaction,
                cancellationToken: ct));
    }
}
