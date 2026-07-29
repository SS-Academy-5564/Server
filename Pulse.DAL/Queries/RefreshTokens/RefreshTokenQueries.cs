using System.Data;
using Dapper;
using Pulse.DAL.Connection;

namespace Pulse.DAL.Queries.RefreshTokens;

/// <summary>
/// Provides methods to query refresh token data from the database.
/// </summary>
public class RefreshTokenQueries : IRefreshTokenQueries
{
    private readonly IDbConnectionFactory _connectionFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RefreshTokenQueries"/> class.
    /// </summary>
    /// <param name="connectionFactory">The database connection factory used to create connections.</param>
    public RefreshTokenQueries(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    /// <summary>
    /// Retrieves a refresh token record by its hashed token string.
    /// </summary>
    /// <param name="tokenHash">The hash of the refresh token.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The refresh token record if found; otherwise, null.</returns>
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
