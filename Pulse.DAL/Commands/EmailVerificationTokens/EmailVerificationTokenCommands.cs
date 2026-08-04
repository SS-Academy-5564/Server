using System.Data;
using Dapper;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Commands.EmailVerificationTokens;

/// <summary>
/// Persists and atomically consumes email verification tokens.
/// </summary>
public sealed class EmailVerificationTokenCommands : IEmailVerificationTokenCommands
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDbSessionAccessor _sessionAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="EmailVerificationTokenCommands"/> class.
    /// </summary>
    /// <param name="connectionFactory">The factory used to create standalone database connections.</param>
    /// <param name="sessionAccessor">The accessor for the active unit-of-work session.</param>
    public EmailVerificationTokenCommands(
        IDbConnectionFactory connectionFactory,
        IDbSessionAccessor sessionAccessor)
    {
        _connectionFactory = connectionFactory;
        _sessionAccessor = sessionAccessor;
    }

    /// <inheritdoc/>
    public async Task CreateAsync(CreateEmailVerificationTokenInput input, CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        const string sql = """
            INSERT INTO EmailVerificationTokens (UserId, TokenHash, ExpiresAt, CreatedAt)
            VALUES (@UserId, @TokenHash, @ExpiresAt, @CreatedAt);
            """;

        await session.Connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                input,
                transaction: session.Transaction,
                cancellationToken: ct));
    }

    /// <inheritdoc/>
    public async Task<EmailVerificationTokenConsumeResult> ConsumeAsync(
        string tokenHash,
        DateTimeOffset consumedAt,
        CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        const string sql = """
            SET XACT_ABORT ON;
            BEGIN TRAN;

            DECLARE @Status INT;
            DECLARE @UserId UNIQUEIDENTIFIER;
            DECLARE @ExpiresAt DATETIMEOFFSET;
            DECLARE @UsedAt DATETIMEOFFSET;

            SELECT
                @UserId = UserId,
                @ExpiresAt = ExpiresAt,
                @UsedAt = UsedAt
            FROM EmailVerificationTokens WITH (UPDLOCK, HOLDLOCK)
            WHERE TokenHash = @TokenHash;

            IF @UserId IS NULL
            BEGIN
                SET @Status = 1;
            END
            ELSE IF @UsedAt IS NOT NULL
            BEGIN
                SET @Status = 3;
            END
            ELSE IF @ExpiresAt <= @ConsumedAt
            BEGIN
                SET @Status = 2;
            END
            ELSE
            BEGIN
                UPDATE EmailVerificationTokens
                SET UsedAt = @ConsumedAt
                WHERE TokenHash = @TokenHash;

                UPDATE Users
                SET EmailVerifiedAt = COALESCE(EmailVerifiedAt, @ConsumedAt),
                    UpdatedAt = @ConsumedAt
                WHERE Id = @UserId;

                SET @Status = 0;
            END

            COMMIT TRAN;
            SELECT @Status;
            """;

        int status = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(
                sql,
                new { TokenHash = tokenHash, ConsumedAt = consumedAt },
                cancellationToken: ct));

        return (EmailVerificationTokenConsumeResult)status;
    }
}
