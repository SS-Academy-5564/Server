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

    /// <inheritdoc/>
    public async Task<bool> TryReserveLoginAttemptAsync(
        Guid userId,
        int maxAttempts,
        DateTime now,
        DateTime lockedUntil,
        CancellationToken ct)
    {
        using IDbConnection connection = _dbConnectionFactory.CreateConnection();

        const string sql =
            """
            SET XACT_ABORT ON;

            BEGIN TRY
                BEGIN TRANSACTION;

                DECLARE @AttemptCount INT;
                DECLARE @CurrentLockedUntil DATETIME2;
                DECLARE @IsAllowed BIT = 0;

                SELECT
                    @AttemptCount = AttemptCount,
                    @CurrentLockedUntil = LockedUntil
                FROM UserLoginAttempts WITH (UPDLOCK, HOLDLOCK)
                WHERE UserId = @UserId;

                IF @AttemptCount IS NULL
                BEGIN
                    INSERT INTO UserLoginAttempts (UserId, AttemptCount, LockedUntil)
                    VALUES (
                        @UserId,
                        1,
                        CASE
                            WHEN @MaxAttempts <= 1 THEN @LockedUntil
                            ELSE NULL
                        END);

                    SET @IsAllowed = 1;
                END
                ELSE IF @CurrentLockedUntil IS NULL OR @CurrentLockedUntil <= @Now
                BEGIN
                    DECLARE @NextAttemptCount INT =
                        CASE
                            WHEN @CurrentLockedUntil IS NOT NULL THEN 1
                            ELSE @AttemptCount + 1
                        END;

                    UPDATE UserLoginAttempts
                    SET
                        AttemptCount = @NextAttemptCount,
                        LockedUntil =
                            CASE
                                WHEN @NextAttemptCount >= @MaxAttempts
                                THEN @LockedUntil
                                ELSE NULL
                            END
                    WHERE UserId = @UserId;

                    SET @IsAllowed = 1;
                END;

                COMMIT TRANSACTION;

                SELECT @IsAllowed;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0
                    ROLLBACK TRANSACTION;

                THROW;
            END CATCH;
            """;

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new
                {
                    UserId = userId,
                    MaxAttempts = maxAttempts,
                    Now = now,
                    LockedUntil = lockedUntil
                },
                cancellationToken: ct));
    }

    /// <inheritdoc/>
    public async Task ResetAttemptsAsync(Guid userId, CancellationToken ct)
    {
        using IDbConnection connection = _dbConnectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            new CommandDefinition(
                "UPDATE UserLoginAttempts " +
                "SET LockedUntil = NULL, AttemptCount = 0 " +
                "WHERE UserId = @userId",
                new { userId },
                cancellationToken: ct)
        );
    }
}
