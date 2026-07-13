using System.Data.Common;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.Monitors;

public class MonitorQueries : IMonitorQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MonitorQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<MonitorRecord>> GetAllAsync(MonitorStatus? status, CancellationToken ct)
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
                return new List<MonitorRecord>().AsReadOnly();
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

        IEnumerable<MonitorRecord> records = await connection.QueryAsync<MonitorRecord>(
            new CommandDefinition(sql, new { StatusId = statusId }, cancellationToken: ct));

        return records.ToList().AsReadOnly();
    }
}

