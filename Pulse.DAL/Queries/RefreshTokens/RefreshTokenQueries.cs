using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.RefreshTokens;

public class RefreshTokenQueries : IRefreshTokenQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RefreshTokenQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RefreshTokenRecord?> GetByTokenHashAsync(string tokenHash, CancellationToken ct)
    {
        using IDbConnection connection = _connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<RefreshTokenRecord>(
            new CommandDefinition(
                """
                SELECT
                    Id,
                    UserId,
                    TokenHash,
                    FamilyId,
                    CreatedAt,
                    ExpiresAt,
                    UsedAt,
                    RevokedAt,
                    ReplacedByTokenId,
                    RevocationReason
                FROM RefreshTokens
                WHERE TokenHash = @TokenHash;
                """,
                new { TokenHash = tokenHash },
                cancellationToken: ct));
    }
}
