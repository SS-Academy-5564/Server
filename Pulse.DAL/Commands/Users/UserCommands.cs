using Dapper;
using Microsoft.Data.SqlClient;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Connection;
using Pulse.DAL.Exceptions;

namespace Pulse.DAL.Commands.Users;

public class UserCommands : IUserCommands
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserCommands(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // change to Task later when we will remove adding user to default organization
    /// <inheritdoc/>
    public async Task<Guid> CreateUserAsync(CreateUserInput input, IUnitOfWork uow, CancellationToken ct)
    {
        try
        {
            return await uow.Connection.ExecuteScalarAsync<Guid>(
                new CommandDefinition(
                    "INSERT INTO Users (Email, FirstName, LastName, PasswordHash, CreatedAt, UpdatedAt) OUTPUT INSERTED.Id VALUES (@Email, @FirstName, @LastName, @PasswordHash, @Now, @Now)",
                    new { input.Email, input.FirstName, input.LastName, input.PasswordHash, Now = DateTimeOffset.UtcNow },
                    transaction: uow.Transaction,
                    cancellationToken: ct));
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new DuplicateKeyException("Email");
        }
    }

    /// <inheritdoc/>
    public async Task UpdatePasswordAsync(Guid userId, string passwordHash, CancellationToken ct)
    {
        using System.Data.IDbConnection connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                "UPDATE Users SET PasswordHash = @PasswordHash, UpdatedAt = @Now WHERE Id = @UserId",
                new { UserId = userId, PasswordHash = passwordHash, Now = DateTimeOffset.UtcNow },
                cancellationToken: ct));
    }
}
