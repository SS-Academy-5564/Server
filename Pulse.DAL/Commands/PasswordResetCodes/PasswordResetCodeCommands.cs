using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Commands.PasswordResetCodes;

/// <inheritdoc/>
public class PasswordResetCodeCommands : IPasswordResetCodeCommands
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PasswordResetCodeCommands(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <inheritdoc/>
    public async Task<Guid> CreateAsync(PasswordResetCodeInput input, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                "INSERT INTO PasswordResetCodes (UserId, CodeHash, ExpiresAt, FailedAttempts) " +
                "OUTPUT INSERTED.Id " +
                "VALUES (@UserId, @CodeHash, @ExpiresAt, 0)",
                new { input.UserId, input.CodeHash, input.ExpiresAt },
                cancellationToken: ct));
    }

    /// <inheritdoc/>
    public async Task DeleteByUserIdAsync(Guid userId, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM PasswordResetCodes WHERE UserId = @UserId",
                new { UserId = userId },
                cancellationToken: ct));
    }

    /// <inheritdoc/>
    public async Task<int> IncrementFailedAttemptsAsync(Guid id, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                "UPDATE PasswordResetCodes " +
                "SET FailedAttempts = FailedAttempts + 1 " +
                "OUTPUT INSERTED.FailedAttempts " +
                "WHERE Id = @Id",
                new { Id = id },
                cancellationToken: ct));
    }
}
