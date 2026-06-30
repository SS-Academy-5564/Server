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
    public async Task<Guid> ReplaceAsync(PasswordResetCodeInput input, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        string sql = @"
            SET XACT_ABORT ON;
            BEGIN TRAN;
            DELETE FROM PasswordResetCodes WHERE UserId = @UserId;
            INSERT INTO PasswordResetCodes (UserId, CodeHash, ExpiresAt, FailedAttempts)
            OUTPUT INSERTED.Id
            VALUES (@UserId, @CodeHash, @ExpiresAt, 0);
            COMMIT TRAN;
        ";

        return await connection.ExecuteScalarAsync<Guid>(
            new CommandDefinition(
                sql,
                new { input.UserId, input.CodeHash, input.ExpiresAt },
                cancellationToken: ct));
    }

    /// <inheritdoc/>
    public async Task<bool> MarkAsVerifiedAsync(Guid id, string jti, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        int rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(
                "UPDATE PasswordResetCodes SET Jti = @Jti WHERE Id = @Id AND Jti IS NULL",
                new { Id = id, Jti = jti },
                cancellationToken: ct));
        
        return rowsAffected == 1;
    }

    /// <inheritdoc/>
    public async Task<bool> DeleteByIdAsync(Guid id, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        int rowsAffected = await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM PasswordResetCodes WHERE Id = @Id",
                new { Id = id },
                cancellationToken: ct));
        
        return rowsAffected == 1;
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
