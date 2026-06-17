
namespace Pulse.BL.Common.Security;

public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a plain-text password using a secure one-way algorithm.
    /// </summary>
    /// <param name="password">The plain-text password to hash.</param>
    /// <returns>The hashed representation of the password.</returns>
    string HashPassword(string password);

    /// <summary>
    /// Verifies that a plain-text password matches a previously hashed password.
    /// </summary>
    /// <param name="hashedPassword">The stored hashed password.</param>
    /// <param name="providedPassword">The plain-text password provided by the user.</param>
    /// <returns><c>true</c> if the password matches the hash; otherwise <c>false</c>.</returns>
    bool VerifyHashedPassword(string hashedPassword, string providedPassword);
}
