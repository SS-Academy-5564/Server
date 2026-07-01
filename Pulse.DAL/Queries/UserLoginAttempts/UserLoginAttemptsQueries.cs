using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.UserLoginAttempts;

/// <inheritdoc cref="IUserLoginAttemptsQueries"/>
public class UserLoginAttemptsQueries : IUserLoginAttemptsQueries
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public UserLoginAttemptsQueries(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    /// <inheritdoc/>
    public async Task<bool> IsUserAllowedAsync(Guid userId, CancellationToken ct)
    {
        using IDbConnection connection = _dbConnectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                """
                SELECT CAST(
                    CASE WHEN EXISTS (
                        SELECT 1
                        FROM UserLoginAttempts
                        WHERE UserId = @UserId
                          AND LockedUntil > SYSUTCDATETIME()
                    )
                    THEN 0
                    ELSE 1
                    END
                    AS BIT)
                """,
                new { UserId = userId },
                cancellationToken: ct));
    }

    /// <inheritdoc/>
    public async Task<UserLoginAttemptsRecord?> GetUserLoginAttemptsAsync(Guid userId, CancellationToken ct)
    {
        using IDbConnection connection = _dbConnectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<UserLoginAttemptsRecord?>(
            new CommandDefinition(
                "SELECT ula.UserId, ula.FailedAttempts, ula.LockedUntil " +
                "FROM UserLoginAttempts AS ula " +
                " WHERE ula.UserId = @userId",
                    new { userId },
                cancellationToken: ct));
    }
}
