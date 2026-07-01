using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Commands.LoginAttempts;

/// <inheritdoc cref="IUserLoginAttemptsCommands"/>
public class UserLoginAttemptsCommands : IUserLoginAttemptsCommands
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    public UserLoginAttemptsCommands(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    /// <inheritdoc/>
    public async Task AddFailedAttemptAsync(
        Guid userId,
        int maxFailedAttempts,
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

                UPDATE UserLoginAttempts WITH (UPDLOCK, HOLDLOCK)
                SET
                    FailedAttempts =
                        CASE
                            WHEN LockedUntil IS NOT NULL AND LockedUntil <= @Now
                            THEN 1
                            ELSE FailedAttempts + 1
                        END,
                    LockedUntil =
                        CASE
                            WHEN
                                CASE
                                    WHEN LockedUntil IS NOT NULL AND LockedUntil <= @Now
                                    THEN 1
                                    ELSE FailedAttempts + 1
                                END >= @MaxFailedAttempts
                            THEN @LockedUntil
                            ELSE NULL
                        END
                WHERE UserId = @UserId;

                IF @@ROWCOUNT = 0
                BEGIN
                    INSERT INTO UserLoginAttempts (UserId, FailedAttempts, LockedUntil)
                    VALUES (
                        @UserId,
                        1,
                        CASE
                            WHEN @MaxFailedAttempts <= 1 THEN @LockedUntil
                            ELSE NULL
                        END);
                END;

                COMMIT TRANSACTION;
            END TRY
            BEGIN CATCH
                IF @@TRANCOUNT > 0
                    ROLLBACK TRANSACTION;

                THROW;
            END CATCH;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    UserId = userId,
                    MaxFailedAttempts = maxFailedAttempts,
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
                "SET LockedUntil = NULL, FailedAttempts = 0 " +
                "WHERE UserId = @userId",
                new { userId },
                cancellationToken: ct)
        );
    }
}
