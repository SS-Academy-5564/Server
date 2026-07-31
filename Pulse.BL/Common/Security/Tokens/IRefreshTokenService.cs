namespace Pulse.BL.Common.Security.Tokens;

/// <summary>
/// Defines the service for managing refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a new, random refresh token string.
    /// </summary>
    /// <returns>The generated refresh token string.</returns>
    string GenerateToken();

    /// <summary>
    /// Computes the hash of a given refresh token string.
    /// </summary>
    /// <param name="token">The refresh token string to hash.</param>
    /// <returns>The hashed token string.</returns>
    string ComputeHash(string token);
}
