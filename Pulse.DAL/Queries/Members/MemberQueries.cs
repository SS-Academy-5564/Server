using System.Data;
using Dapper;
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
}
