namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Generates secure email verification tokens and computes their persistence-safe hashes.
/// </summary>
public interface IEmailVerificationTokenService
{
    /// <summary>
    /// Generates a cryptographically secure URL-safe token.
    /// </summary>
    /// <returns>A new opaque verification token.</returns>
    string GenerateToken();

    /// <summary>
    /// Computes the SHA-256 hash used to identify a token without persisting it in plaintext.
    /// </summary>
    /// <param name="token">The raw token presented by the user.</param>
    /// <returns>The hexadecimal SHA-256 hash.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="token"/> is null, empty, or whitespace.</exception>
    string ComputeHash(string token);
}
