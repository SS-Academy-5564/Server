using Dapper;
using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Commands.Monitors;

/// <inheritdoc cref="IMonitorCommands"/>
public class MonitorCommands : IMonitorCommands
{
    private readonly IDbSessionAccessor _sessionAccessor;

    public MonitorCommands(IDbSessionAccessor sessionAccessor)
    {
        _sessionAccessor = sessionAccessor;
    }

    ///<inheritdoc/>
    public async Task<Guid> CreateAsync(CreateMonitorInput input, CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        const string sql =
            """
            INSERT INTO dbo.Monitors
                (Name, Url, HttpMethod, ResultPath, StatusId, PollingIntervalSeconds, PollingTimeoutSeconds)
            OUTPUT INSERTED.Id
            VALUES
                (
                    @Name,
                    @Url,
                    (SELECT Id FROM dbo.HttpMethods WHERE Name = @HttpMethod),
                    @ResultPath,
                    (SELECT Id FROM dbo.MonitorStatuses WHERE Name = 'Enabled'),
                    @PollingIntervalSeconds,
                    @PollingTimeoutSeconds
                );
            """;

        return await session.Connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                sql,
                input,
                transaction: session.Transaction,
                cancellationToken: ct));
    }

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
