
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.Users;

public class UserQueries : IUserQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT CAST(1 AS BIT) FROM Users WHERE Email = @Email",
                new { Email = email },
                cancellationToken: ct));
    }

    public async Task<UserAuthRecord?> GetByEmailForAuthAsync(string email, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<UserAuthRecord>(
            new CommandDefinition(
                "SELECT TOP(1) u.Id, u.Email, u.PasswordHash, m.OrganizationId, r.Name AS RoleName " +
                "FROM Users u " +
                "JOIN Members m ON m.UserId = u.Id " +
                "JOIN Roles r ON r.Id = m.RoleId " +
                "WHERE u.Email = @Email " +
                "ORDER BY m.JoinedAt DESC",
                new { Email = email },
                cancellationToken: ct));
    }
}
