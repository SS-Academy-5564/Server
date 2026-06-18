using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Pulse.DAL.Exceptions;

namespace Pulse.DAL.Commands.Users;

public class UserCommands : IUserCommands
{
    // change to Task later when we will remove adding user to default organization
    /// <inheritdoc/>
    public async Task<Guid> CreateUserAsync(CreateUserInput input, IDbTransaction transaction, CancellationToken ct)
    {
        try
        {
            return await transaction.Connection!.ExecuteScalarAsync<Guid>(
                new CommandDefinition(
                    "INSERT INTO Users (Email, FirstName, LastName, PasswordHash, CreatedAt, UpdatedAt) OUTPUT INSERTED.Id VALUES (@Email, @FirstName, @LastName, @PasswordHash, @Now, @Now)",
                    new { input.Email, input.FirstName, input.LastName, input.PasswordHash, Now = DateTimeOffset.UtcNow },
                    transaction: transaction,
                    cancellationToken: ct));
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            throw new DuplicateKeyException("Email");
        }
    }
}
