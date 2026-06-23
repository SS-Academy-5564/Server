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
    GeneratedJwtToken GenerateToken(Guid userId, string roleName, Guid organizationId);
}
