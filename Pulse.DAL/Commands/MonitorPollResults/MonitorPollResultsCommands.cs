using Dapper;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.MonitorPollResults;

public class MonitorPollResultsCommands : IMonitorPollResultsCommands
{
    public async Task CreateAsync(CreateMonitorPollResultsInput monitorPollResultsInput, IUnitOfWork uow, CancellationToken ct)
    {
        const string sql =
            """
            INSERT INTO dbo.MonitorPollResults
                (Value, CheckedAt, IsSuccess, ResponseTimeMs, StatusCode, ErrorMessage, MonitorId, RequestStatusId)
            VALUES
                (
                    @Value,
                    @CheckedAt,
                    @IsSuccess,
                    @ResponseTimeMs,
                    @StatusCode,
                    @ErrorMessage,
                    @MonitorId,
                    (SELECT Id FROM dbo.RequestStatus WHERE Status = @RequestStatus)
                );
            """;

        await uow.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                monitorPollResultsInput,
                transaction: uow.Transaction,
                cancellationToken: ct));
    }
}
