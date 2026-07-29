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
            $$"""
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
            WHERE m.OrganizationId = @OrganizationId
            {{whereClause}}
            ORDER BY m.Id
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT COUNT(*)
            FROM dbo.Monitors AS m
            JOIN dbo.MonitorStatuses AS s ON m.StatusId = s.Id
            WHERE m.OrganizationId = @OrganizationId
            {{whereClause}};
            """;

        using SqlMapper.GridReader result = await connection.QueryMultipleAsync(new(
            sql,
            parameters,
            cancellationToken: ct));

        IReadOnlyList<MonitorListRecord> records = (await result.ReadAsync<MonitorListRecord>()).ToList().AsReadOnly();
        int totalCount = await result.ReadSingleAsync<int>();

        return new PagedRecords<MonitorListRecord>(records, totalCount);
    }

    public async Task<IEnumerable<MonitorPollingRecord>> GetDueEnabledAsync(Guid? organizationId, int max, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        string orgFilter = organizationId.HasValue
            ? "WHERE m.OrganizationId = @OrganizationId "
            : string.Empty;

        return await connection.QueryAsync<MonitorPollingRecord>(
            new CommandDefinition(
                $"SELECT TOP (@Max) m.Id, m.Url, h.Name AS HttpMethod, m.ResultPath, m.PollingIntervalSeconds, m.PollingTimeoutSeconds, s.Name AS Status, m.OrganizationId " +
                $"FROM Monitors AS m " +
                $"JOIN HttpMethods AS h ON m.HttpMethod = h.Id " +
                $"JOIN MonitorStatuses AS s ON m.StatusId = s.Id " +
                $"{orgFilter}" +
                $"AND m.NextExecutionAt <= SYSUTCDATETIME() " +
                $"AND s.Name = 'Enabled' " +
                $"Order By m.NextExecutionAt ASC;",
                new { Max = max, OrganizationId = organizationId },
                cancellationToken: ct)
        );
    }

    /// <inheritdoc/>
    public async Task<MonitorPollingRecord?> GetByIdForPollingAsync(Guid id, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<MonitorPollingRecord>(
            new CommandDefinition(
                "SELECT m.Id, m.Url, h.Name AS HttpMethod, m.ResultPath, m.PollingIntervalSeconds, m.PollingTimeoutSeconds, s.Name AS Status, m.OrganizationId " +
                "FROM Monitors AS m " +
                "JOIN HttpMethods AS h ON m.HttpMethod = h.Id " +
                "JOIN MonitorStatuses AS s ON m.StatusId = s.Id " +
                "WHERE m.Id = @Id " +
                "   AND s.Name IN ('Enabled', 'Error');",
                new { Id = id },
                cancellationToken: ct));
    }
}
