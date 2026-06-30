using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Commands.LoginAttempts;

public class UserLoginAttemptsCommands : IUserLoginAttemptsCommands
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public UserLoginAttemptsCommands(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task ResetAttemptsAsync(Guid userId, CancellationToken ct)
    {
        using IDbConnection connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                "UPDATE UserLoginAttempts " +
                "SET LockedUntil = NULL, Attempts = 0 " +
                "WHERE UserId = @userId",
                new { userId },
                cancellationToken: ct)
        );
    }
}
