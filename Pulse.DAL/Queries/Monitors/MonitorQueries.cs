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
        Guid organizationId,
        MonitorStatus? status,
        int pageNumber,
        int pageSize,
        string? searchString,
        CancellationToken ct)
    {
        using DbConnection connection = _connectionFactory.CreateConnection();
        int offset = checked((pageNumber - 1) * pageSize);

        var filters = new List<string>();
        var parameters = new DynamicParameters();

        parameters.Add("OrganizationId", organizationId);
        parameters.Add("Offset", offset);
        parameters.Add("PageSize", pageSize);

        filters.Add("m.OrganizationId = @OrganizationId");

        if (status.HasValue)
        {
            filters.Add("s.Name = @Status");
            parameters.Add("Status", status.ToString());
        }

        if (!string.IsNullOrWhiteSpace(searchString))
        {
            filters.Add("m.Name LIKE @SearchString");
            parameters.Add("SearchString", $"%{searchString.Trim()}%");
        }

        string whereClause = filters.Count > 0
            ? $"WHERE {string.Join(" AND ", filters)}"
            : string.Empty;

        string sql =
            $"""
            DECLARE @TotalCount AS INT = (
                SELECT COUNT(*)
                FROM dbo.Monitors AS m
                JOIN dbo.MonitorStatuses AS s ON m.StatusId = s.Id
                {whereClause}
            );

            IF @TotalCount > 0 AND @Offset >= @TotalCount
            BEGIN
                SET @Offset = ((@TotalCount - 1) / @PageSize) * @PageSize;
            END;

            SELECT
                m.Id,
                m.Name,
                m.Url,
                m.CurrentValue,
                m.LastCheckedAt,
                s.Name AS Status,
                m.PollingIntervalSeconds AS Interval,
                m.OrganizationId
            FROM dbo.Monitors AS m
            JOIN dbo.MonitorStatuses AS s ON m.StatusId = s.Id
            {whereClause}
            ORDER BY m.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT @TotalCount AS TotalCount;
            """;

        using SqlMapper.GridReader result = await connection.QueryMultipleAsync(new(
            sql,
            parameters,
            cancellationToken: ct));

        IReadOnlyList<MonitorListRecord> records = (await result.ReadAsync<MonitorListRecord>()).ToList().AsReadOnly();
        int totalCount = await result.ReadSingleAsync<int>();

        return new PagedRecords<MonitorListRecord>(records, totalCount);
    }

    /// <inheritdoc/>
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
                "AND s.Name = 'Enabled' " +
                "ORDER BY m.NextExecutionAt ASC;",
                new { Max = max },
                cancellationToken: ct)
        );
    }

    /// <inheritdoc/>
    public async Task<MonitorRecord?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        const string sql = """
            SELECT
                m.Id,
                m.OrganizationId,
                m.Name,
                m.Url,
                h.Name AS HttpMethod,
                m.ResultPath,
                m.CurrentValue,
                s.Name AS Status,
                m.PollingIntervalSeconds,
                m.PollingTimeoutSeconds,
                m.LastCheckedAt,
                m.NextExecutionAt,
                m.CreatedAt,
                m.LastModifiedAt
            FROM dbo.Monitors m
            JOIN HttpMethods AS h ON m.HttpMethod = h.Id
            JOIN MonitorStatuses AS s ON m.StatusId = s.Id
            WHERE m.Id = @Id;
            """;

        return await connection.QuerySingleOrDefaultAsync<MonitorRecord>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
    }

    public async Task<MonitorPollingRecord?> GetByIdForPollingAsync(Guid id, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<MonitorPollingRecord>(
            new CommandDefinition(
                "SELECT m.Id, m.Url, h.Name AS HttpMethod, m.ResultPath, m.PollingIntervalSeconds, m.PollingTimeoutSeconds, s.Name AS Status " +
                "FROM Monitors AS m " +
                "JOIN HttpMethods AS h ON m.HttpMethod = h.Id " +
                "JOIN MonitorStatuses AS s ON m.StatusId = s.Id " +
                "WHERE m.Id = @Id " +
                "   AND s.Name IN ('Enabled', 'Error');",
                new { Id = id },
                cancellationToken: ct));
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<MonitorLookupRecord>> GetMonitorsLookupAsync(Guid organizationId, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.QueryAsync<MonitorLookupRecord>(
            new CommandDefinition(
                "SELECT m.Name, m.Id " +
                "FROM Monitors AS m " +
                "WHERE m.OrganizationId = @organizationId",
                new { organizationId }, cancellationToken: ct));
    }

    public async Task<ILookup<MonitorMetric, decimal>> GetMonitorsStatisticsAsync(
        IEnumerable<MonitorMetric> monitors,
        CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        IReadOnlyList<MonitorMetric> monitorList = monitors.Distinct().ToList();
        var allRecords = new List<(MonitorMetric Monitor, decimal Value)>();

        foreach (IGrouping<(MetricType Metric, DateTimeOffset From), MonitorMetric> group in
            monitorList.GroupBy(m => (m.Metric, m.From)))
        {
            IEnumerable<Guid> ids = group.Select(m => m.MonitorId).Distinct();
            string sql = BuildStatisticsSql(group.Key.Metric);

            IEnumerable<(Guid MonitorId, decimal Value)> rows =
                await connection.QueryAsync<(Guid MonitorId, decimal Value)>(
                    new CommandDefinition(
                        sql,
                        new { Ids = ids, group.Key.From },
                        cancellationToken: ct));

            foreach ((Guid MonitorId, decimal Value) row in rows)
            {
                MonitorMetric? match = group.FirstOrDefault(m => m.MonitorId == row.MonitorId);
                if (match is not null)
                {
                    allRecords.Add((match, row.Value));
                }
            }
        }

        return allRecords.ToLookup(r => r.Monitor, r => r.Value);
    }

    private static string BuildStatisticsSql(MetricType metric)
        => metric switch
        {
            MetricType.Availability =>
                "SELECT MonitorId, " +
                "CAST(ROUND(100.0 * SUM(CAST(IsSuccess AS INT)) / COUNT(*), 2) AS DECIMAL(18,2)) AS Value " +
                "FROM MonitorPollResults " +
                "WHERE MonitorId IN @Ids AND CheckedAt >= @From " +
                "GROUP BY MonitorId",

            MetricType.Requests =>
                "SELECT MonitorId, " +
                "CAST(COUNT(*) AS DECIMAL(18,2)) AS Value " +
                "FROM MonitorPollResults " +
                "WHERE MonitorId IN @Ids AND CheckedAt >= @From " +
                "GROUP BY MonitorId",

            MetricType.Errors =>
                "SELECT MonitorId, " +
                "CAST(SUM(CASE WHEN IsSuccess = 0 THEN 1 ELSE 0 END) AS DECIMAL(18,2)) AS Value " +
                "FROM MonitorPollResults " +
                "WHERE MonitorId IN @Ids AND CheckedAt >= @From " +
                "GROUP BY MonitorId",

            _ =>
                "SELECT MonitorId, " +
                "CAST(ResponseTimeMs AS DECIMAL(18,2)) AS Value " +
                "FROM MonitorPollResults " +
                "WHERE MonitorId IN @Ids AND CheckedAt >= @From",
        };
}
