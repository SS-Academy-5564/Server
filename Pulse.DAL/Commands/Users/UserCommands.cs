
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

    // change to Task later, so code will follow command/query segregation principles
    public async Task<Guid> CreateAsync(CreateUserInput input, CancellationToken ct)
    {
        using var connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                "INSERT INTO Users (Email, PasswordHash, CreatedAt, UpdatedAt) OUTPUT INSERTED.Id VALUES (@Email, @PasswordHash, @Now, @Now)",
                new { Email = input.Email, PasswordHash = input.PasswordHash, Now = DateTimeOffset.UtcNow },
                cancellationToken: ct));
    }
}
