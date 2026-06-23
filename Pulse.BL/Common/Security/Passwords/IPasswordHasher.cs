namespace Pulse.BL.Common.Security.Passwords;

/// <summary>
/// Provides password hashing and verification operations.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password using the configured hashing algorithm.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>The hashed password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies whether a provided password matches a hashed password.
    /// </summary>
    /// <param name="hashedPassword">The stored hashed password.</param>
    /// <param name="providedPassword">The plaintext password supplied by the user.</param>
    /// <returns><c>true</c> when the password matches; otherwise <c>false</c>.</returns>
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}
