using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.Monitors;

public class MonitorQueries : IMonitorQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MonitorQueries(IDbConnectionFactory factory)
    {
        _connectionFactory = factory;
    }

    public async Task<IEnumerable<MonitorRecord>> GetDueEnabledAsync(int max, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<MonitorRecord>(
            new CommandDefinition(
                "SELECT TOP (@Max) m.Id, m.Url, h.Name AS HttpMethod, m.ResultPath, m.PollingIntervalSeconds, m.PollingTimeoutSeconds " +
                "FROM Monitors AS m " +
                "JOIN HttpMethods AS h ON m.HttpMethod = h.Id " +
                "WHERE m.NextExecutionAt <= SYSUTCDATETIME() AND m.StatusId = " +
                "   (SELECT Id FROM MonitorStatuses WHERE Name = 'Enabled') " +
                "Order By m.NextExecutionAt ASC;",
                new { Max = max },
                cancellationToken: ct)
        );
    }
}
