using System.Data;
using System.Data.Common;
using Dapper;
using Pulse.DAL.Common.Pagination;
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
    public async Task<PagedRecords<MonitorListRecord>> GetAllAsync(
        MonitorStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        using DbConnection connection = _connectionFactory.CreateConnection();
        int offset = checked((pageNumber - 1) * pageSize);

        string statusFilter = status is null ? string.Empty : "WHERE s.Name = @Status";
        string sql =
            $$"""
            SELECT
                m.Id,
                m.Name,
                m.Url,
                m.CurrentValue,
                m.LastCheckedAt,
                s.Name AS Status,
                m.PollingIntervalSeconds AS Interval
            FROM dbo.Monitors AS m
            JOIN dbo.MonitorStatuses AS s ON m.StatusId = s.Id
            {{statusFilter}}
            ORDER BY m.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM dbo.Monitors AS m
            JOIN dbo.MonitorStatuses AS s ON m.StatusId = s.Id
            {{statusFilter}};
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                Status = status?.ToString(),
                Offset = offset,
                PageSize = pageSize
            },
            cancellationToken: ct);

        using SqlMapper.GridReader result = await connection.QueryMultipleAsync(command);
        IReadOnlyList<MonitorListRecord> records = (await result.ReadAsync<MonitorListRecord>()).ToList().AsReadOnly();
        int totalCount = await result.ReadSingleAsync<int>();

        return new PagedRecords<MonitorListRecord>(records, totalCount);
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
