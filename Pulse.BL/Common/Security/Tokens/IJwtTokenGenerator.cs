namespace Pulse.BL.Common.Security.Tokens;

/// <summary>
/// Represents a generated JSON Web Token and its expiration time.
/// </summary>
/// <param name="Token">The serialized JWT token.</param>
/// <param name="ExpiresAt">The time at which the token expires.</param>
public sealed record GeneratedJwtToken(string Token, DateTimeOffset ExpiresAt);

/// <summary>
/// Generates JWT tokens for authenticated users.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates a JWT token for the specified user context.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="roleName">The role assigned to the user.</param>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <returns>A generated JWT token along with its expiration time.</returns>
    GeneratedJwtToken GenerateToken(Guid userId, string roleName, Guid organizationId, string organizationName);

    /// <summary>
    /// Generates a short-lived password reset token for the given user, tied to a specific session JTI.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="jti">The unique JWT ID for the reset session.</param>
    /// <param name="expiresIn">How long the reset token should be valid.</param>
    /// <returns>A signed JWT intended only for the password reset flow.</returns>
    string GeneratePasswordResetToken(Guid userId, string jti, TimeSpan expiresIn);

    /// <summary>
    /// Validates a password reset token and extracts the user ID and JTI.
    /// </summary>
    /// <param name="token">The token to validate.</param>
    /// <returns>The user ID and JTI embedded in the token, or <c>null</c> if the token is invalid or expired.</returns>
    Task<(Guid UserId, string Jti)?> ValidatePasswordResetTokenAsync(string token);
}
