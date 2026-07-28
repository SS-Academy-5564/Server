using System.Data;
using Dapper;
using Pulse.DAL.Connection;
using Pulse.DAL.Queries.RefreshTokens;

namespace Pulse.DAL.Commands.RefreshTokens;

public class RefreshTokenCommands : IRefreshTokenCommands
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RefreshTokenCommands(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(RefreshTokenRecord record, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                INSERT INTO RefreshTokens (
                    Id, UserId, TokenHash, FamilyId, CreatedAt, ExpiresAt, UsedAt, RevokedAt, ReplacedByTokenId, RevocationReason
                )
                VALUES (
                    @Id, @UserId, @TokenHash, @FamilyId, @CreatedAt, @ExpiresAt, @UsedAt, @RevokedAt, @ReplacedByTokenId, @RevocationReason
                );
                """,
                record,
                cancellationToken: ct));
    }

    public async Task UpdateAsync(RefreshTokenRecord record, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE RefreshTokens
                SET
                    UsedAt = @UsedAt,
                    RevokedAt = @RevokedAt,
                    ReplacedByTokenId = @ReplacedByTokenId,
                    RevocationReason = @RevocationReason
                WHERE Id = @Id;
                """,
                record,
                cancellationToken: ct));
    }

    public async Task RevokeFamilyAsync(Guid familyId, string reason, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE RefreshTokens
                SET RevokedAt = SYSUTCDATETIME(), RevocationReason = @Reason
                WHERE FamilyId = @FamilyId AND RevokedAt IS NULL AND ExpiresAt > SYSUTCDATETIME();
                """,
                new { FamilyId = familyId, Reason = reason },
                cancellationToken: ct));
    }

    public async Task RevokeAllForUserAsync(Guid userId, string reason, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        await connection.ExecuteAsync(
            new CommandDefinition(
                """
                UPDATE RefreshTokens
                SET RevokedAt = SYSUTCDATETIME(), RevocationReason = @Reason
                WHERE UserId = @UserId AND RevokedAt IS NULL AND ExpiresAt > SYSUTCDATETIME();
                """,
                new { UserId = userId, Reason = reason },
                cancellationToken: ct));
    }
}
