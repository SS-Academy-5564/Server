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

    /// <inheritdoc/>
    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                "SELECT TOP 1 CAST(1 AS BIT) FROM Users WHERE Email = @Email",
                new { Email = email },
                cancellationToken: ct));
    }
}
