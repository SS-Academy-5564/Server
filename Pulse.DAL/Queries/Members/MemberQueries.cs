using System.Data;
using System.Data.Common;
using Dapper;
using Pulse.DAL.Common.Pagination;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.Members;

public class MemberQueries : IMemberQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public MemberQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<UserOrganizationRecord>> GetOrganizationsByUserIdAsync(Guid userId, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        IEnumerable<UserOrganizationRecord> records = await connection.QueryAsync<UserOrganizationRecord>(
            new CommandDefinition(
                "SELECT o.Id AS OrganizationId, o.Name AS OrganizationName, " +
                "r.Id AS RoleId, r.Name AS RoleName, m.JoinedAt " +
                "FROM Members m " +
                "JOIN Organizations o ON o.Id = m.OrganizationId " +
                "JOIN Roles r ON r.Id = m.RoleId " +
                "WHERE m.UserId = @UserId " +
                "ORDER BY m.JoinedAt, m.Id",
                new { UserId = userId },
                cancellationToken: ct));

        return records.ToList();
    }

    /// <inheritdoc/>
    public async Task<PagedRecords<MemberRecord>> GetMembersByOrganizationIdAsync(
        Guid organizationId,
        int pageNumber,
        int pageSize,
        CancellationToken ct)
    {
        using DbConnection connection = _connectionFactory.CreateConnection();
        int offset = checked((pageNumber - 1) * pageSize);

        const string sql = """
            DECLARE @TotalCount AS INT = (
                SELECT COUNT(*)
                FROM Members m
                WHERE m.OrganizationId = @OrganizationId;
                );

            IF @TotalCount > 0 AND @Offset >= @TotalCount
            BEGIN
                SET @Offset = ((@TotalCount - 1) / @PageSize) * @PageSize;
            END;

            SELECT u.Id AS UserId,
                   u.Email,
                   u.FirstName,
                   u.LastName,
                   r.Name AS RoleName,
                   m.JoinedAt
            FROM Members m
            JOIN Users u ON u.Id = m.UserId
            JOIN Roles r ON r.Id = m.RoleId
            WHERE m.OrganizationId = @OrganizationId
            ORDER BY m.JoinedAt ASC, m.Id ASC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

            SELECT @TotalCount AS TotalCount;
            """;

        CommandDefinition command = new(
            sql,
            new
            {
                OrganizationId = organizationId,
                Offset = offset,
                PageSize = pageSize
            },
            cancellationToken: ct);

        using SqlMapper.GridReader result = await connection.QueryMultipleAsync(command);
        IReadOnlyList<MemberRecord> records = (await result.ReadAsync<MemberRecord>()).ToList().AsReadOnly();

        int totalCount = await result.ReadSingleAsync<int>();
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        int effectivePageNumber = totalPages > 0
            ? Math.Min(pageNumber, totalPages)
            : 1;
        return new PagedRecords<MemberRecord>(
            records,
            effectivePageNumber,
            pageSize,
            totalCount);
    }
}
