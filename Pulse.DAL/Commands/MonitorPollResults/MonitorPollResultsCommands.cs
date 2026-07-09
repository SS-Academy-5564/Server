using Dapper;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.MonitorPollResults;

public class MonitorPollResultsCommands : IMonitorPollResultsCommands
{
    private readonly IDbSessionAccessor _sessionAccessor;

    public MonitorPollResultsCommands(IDbSessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }

    public async Task CreateAsync(CreateMonitorPollResultsInput monitorPollResultsInput, CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

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
