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

    /// <inheritdoc/>
    public async Task<EmailVerificationTokenResendPreparation> PrepareResendAsync(
        PrepareEmailVerificationTokenResendInput input,
        CancellationToken ct)
    {
        IDbSession session = _sessionAccessor.Session
            ?? throw new InvalidOperationException("No active unit of work.");

        const string sql = """
            DECLARE @Status INT = 1;
            DECLARE @UserId UNIQUEIDENTIFIER;
            DECLARE @Email NVARCHAR(256);
            DECLARE @PresentedExpiresAt DATETIMEOFFSET;
            DECLARE @PresentedCreatedAt DATETIMEOFFSET;
            DECLARE @PresentedUsedAt DATETIMEOFFSET;
            DECLARE @EmailVerifiedAt DATETIMEOFFSET;
            DECLARE @LatestCreatedAt DATETIMEOFFSET;

            SELECT
                @UserId = tokens.UserId,
                @Email = users.Email,
                @PresentedExpiresAt = tokens.ExpiresAt,
                @PresentedCreatedAt = tokens.CreatedAt,
                @PresentedUsedAt = tokens.UsedAt,
                @EmailVerifiedAt = users.EmailVerifiedAt
            FROM EmailVerificationTokens AS tokens WITH (UPDLOCK, HOLDLOCK)
            INNER JOIN Users AS users WITH (UPDLOCK, HOLDLOCK) ON users.Id = tokens.UserId
            WHERE tokens.TokenHash = @PresentedTokenHash;

            IF @UserId IS NULL
            BEGIN
                SET @Status = 1;
            END
            ELSE IF @PresentedUsedAt IS NOT NULL OR @EmailVerifiedAt IS NOT NULL
            BEGIN
                SET @Status = 3;
            END
            ELSE IF @PresentedExpiresAt > @RequestedAt
            BEGIN
                SET @Status = 2;
            END
            ELSE
            BEGIN
                SELECT @LatestCreatedAt = MAX(CreatedAt)
                FROM EmailVerificationTokens WITH (UPDLOCK, HOLDLOCK)
                WHERE UserId = @UserId;

                IF @LatestCreatedAt > @PresentedCreatedAt
                    AND DATEADD(SECOND, @ResendCooldownSeconds, @LatestCreatedAt) > @RequestedAt
                BEGIN
                    SET @Status = 4;
                END
                ELSE
                BEGIN
                    UPDATE EmailVerificationTokens
                    SET UsedAt = @RequestedAt
                    WHERE UserId = @UserId
                        AND UsedAt IS NULL
                        AND ExpiresAt > @RequestedAt;

                    INSERT INTO EmailVerificationTokens (UserId, TokenHash, ExpiresAt, CreatedAt)
                    VALUES (@UserId, @ReplacementTokenHash, @ReplacementExpiresAt, @RequestedAt);

                    SET @Status = 0;
                END
            END

            SELECT
                @Status AS Status,
                CASE WHEN @Status = 0 THEN @Email ELSE NULL END AS Email;
            """;

        return await session.Connection.QuerySingleAsync<EmailVerificationTokenResendPreparation>(
            new CommandDefinition(
                sql,
                input,
                transaction: session.Transaction,
                cancellationToken: ct));
    }
}
