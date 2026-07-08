using System.Data;
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

    public async Task<IReadOnlyList<MonitorRecord>> GetAllAsync(MonitorStatus? status, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        string sql =
            "SELECT m.Id, m.Name, m.Url, m.CurrentValue, m.LastCheckedAt, s.Name AS Status, m.PollingIntervalSeconds AS Interval " +
            "FROM dbo.Monitors m " +
            "JOIN dbo.MonitorStatuses s ON m.StatusId = s.Id";

        if (status is not null)
        {
            sql += " WHERE s.Name = @Status";
        }

        IEnumerable<MonitorRecord> records = await connection.QueryAsync<MonitorRecord>(
            new CommandDefinition(sql, new { Status = status?.ToString() }, cancellationToken: ct));

        return records.ToList().AsReadOnly();
    }
}
