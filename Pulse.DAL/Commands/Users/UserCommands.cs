
using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Commands.Users;

public class UserCommands : IUserCommands
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserCommands(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // Returning the created identifier allows the caller to use the new user immediately.
    public async Task<Guid> CreateAsync(CreateUserInput input, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                "INSERT INTO Users (Email, FirstName, LastName, PasswordHash, CreatedAt, UpdatedAt) OUTPUT INSERTED.Id VALUES (@Email, @FirstName, @LastName, @PasswordHash, @Now, @Now)",
                new { input.Email, input.FirstName, input.LastName, input.PasswordHash, Now = DateTimeOffset.UtcNow },
                cancellationToken: ct));
    }
}
