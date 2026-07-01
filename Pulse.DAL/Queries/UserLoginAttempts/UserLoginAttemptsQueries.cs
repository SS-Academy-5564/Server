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
