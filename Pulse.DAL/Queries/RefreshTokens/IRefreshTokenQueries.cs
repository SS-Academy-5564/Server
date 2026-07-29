using Pulse.DAL.Common.Repository;

namespace Pulse.DAL.Queries.RefreshTokens;

/// <summary>
/// Defines the queries for retrieving refresh tokens from the database.
/// </summary>
public interface IRefreshTokenQueries : IQueries
{
    /// <summary>
    /// Retrieves a refresh token record by its hashed token string.
    /// </summary>
    /// <param name="tokenHash">The hash of the refresh token.</param>
    /// <param name="ct">The cancellation token.</param>
    /// <returns>The refresh token record if found; otherwise, null.</returns>
    Task<RefreshTokenRecord?> GetByTokenHashAsync(string tokenHash, CancellationToken ct);
}
