using System.Data;
using System.Data.Common;
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

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MonitorListRecord>> GetAllAsync(MonitorStatus? status, CancellationToken ct)
    {
        using DbConnection connection = _connectionFactory.CreateConnection();

        Guid? statusId = null;
        if (status is not null)
        {
            statusId = await connection.ExecuteScalarAsync<Guid?>(
                new CommandDefinition(
                    "SELECT Id FROM dbo.MonitorStatuses WHERE Name = @Name",
                    new { Name = status.Value.ToString() },
                    cancellationToken: ct));

            if (statusId is null)
            {
                return new List<MonitorListRecord>().AsReadOnly();
            }
        }

        string sql =
            "SELECT m.Id, m.Name, m.Url, m.CurrentValue, m.LastCheckedAt, s.Name AS Status, m.PollingIntervalSeconds AS Interval " +
            "FROM dbo.Monitors m " +
            "JOIN dbo.MonitorStatuses s ON m.StatusId = s.Id";

        if (statusId is not null)
        {
            sql += " WHERE m.StatusId = @StatusId";
        }

        IEnumerable<MonitorListRecord> records = await connection.QueryAsync<MonitorListRecord>(
            new CommandDefinition(sql, new { StatusId = statusId }, cancellationToken: ct));

        return records.ToList().AsReadOnly();
    }

    public async Task<IEnumerable<MonitorPollingRecord>> GetDueEnabledAsync(int max, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<MonitorPollingRecord>(
            new CommandDefinition(
                "SELECT TOP (@Max) m.Id, m.Url, h.Name AS HttpMethod, m.ResultPath, m.PollingIntervalSeconds, m.PollingTimeoutSeconds, s.Name AS Status " +
                "FROM Monitors AS m " +
                "JOIN HttpMethods AS h ON m.HttpMethod = h.Id " +
                "JOIN MonitorStatuses AS s ON m.StatusId = s.Id " +
                "WHERE m.NextExecutionAt <= SYSUTCDATETIME() " +
                "   AND s.Name = 'Enabled' " +
                "Order By m.NextExecutionAt ASC;",
                new { Max = max },
                cancellationToken: ct)
        );
    }
}
