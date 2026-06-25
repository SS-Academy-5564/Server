using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.PasswordResetCodes;

/// <inheritdoc/>
public class PasswordResetCodeQueries : IPasswordResetCodeQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PasswordResetCodeQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc/>
    public async Task<PasswordResetCodeRecord?> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<PasswordResetCodeRecord>(
            new CommandDefinition(
                "SELECT TOP(1) Id, UserId, CodeHash, ExpiresAt, FailedAttempts " +
                "FROM PasswordResetCodes " +
                "WHERE UserId = @UserId " +
                "ORDER BY ExpiresAt DESC",
                new { UserId = userId },
                cancellationToken: ct));
    }
}
