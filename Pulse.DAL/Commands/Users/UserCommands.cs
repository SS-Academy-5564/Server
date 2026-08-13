using Dapper;
using Microsoft.Data.SqlClient;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Commands.Users;

public class UserCommands : IUserCommands
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbSessionAccessor _sessionAccessor;

    public UserCommands(IDbConnectionFactory connectionFactory, IDbSessionAccessor sessionAccessor)
    {
        _connectionFactory = connectionFactory;
        _sessionAccessor = sessionAccessor;
    }

    // change to Task later when we will remove adding user to default organization
    /// <inheritdoc/>
    public async Task<CreateUserResult> CreateUserAsync(CreateUserInput input, CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        try
        {
            Guid userId = await session.Connection.ExecuteScalarAsync<Guid>(
                new CommandDefinition(
                    "INSERT INTO Users (Email, FirstName, LastName, PasswordHash, CreatedAt, UpdatedAt) OUTPUT INSERTED.Id VALUES (@Email, @FirstName, @LastName, @PasswordHash, @Now, @Now)",
                    new
                    {
                        input.Email,
                        input.FirstName,
                        input.LastName,
                        input.PasswordHash,
                        Now = DateTimeOffset.UtcNow
                    },
                    transaction: session.Transaction,
                    cancellationToken: ct));

            return new CreateUserResult(CreateUserStatus.Succeeded, userId);
        }
        catch (SqlException ex) when (ex.Number is 2627 or 2601)
        {
            return new CreateUserResult(CreateUserStatus.DuplicateEmail, null);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> ConsumeResetTokenAndUpdatePasswordAsync(Guid userId, string jti, string newPasswordHash,
        CancellationToken ct)
    {
        using System.Data.IDbConnection connection = _connectionFactory.CreateConnection();

        string sql = @"
            SET XACT_ABORT ON;
            BEGIN TRAN;

            DECLARE @Deleted INT;

            DELETE FROM PasswordResetCodes
            WHERE UserId = @UserId AND Jti = @Jti AND ExpiresAt > GETUTCDATE();

            SET @Deleted = @@ROWCOUNT;

            IF @Deleted = 1
            BEGIN
                UPDATE Users
                SET PasswordHash = @PasswordHash, UpdatedAt = @Now
                WHERE Id = @UserId;

                UPDATE RefreshTokens
                SET RevokedAt = @Now, RevocationReason = 'PasswordChanged'
                WHERE UserId = @UserId AND RevokedAt IS NULL AND ExpiresAt > @Now;
            END

            COMMIT TRAN;

            SELECT @Deleted;
        ";

        int result = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new { UserId = userId, Jti = jti, PasswordHash = newPasswordHash, Now = DateTimeOffset.UtcNow },
                cancellationToken: ct));

        return result == 1;
    }
}
