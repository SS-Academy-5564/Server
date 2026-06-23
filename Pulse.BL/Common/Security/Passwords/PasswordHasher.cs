using Microsoft.AspNetCore.Identity;

namespace Pulse.BL.Common.Security.Passwords;

/// <summary>
/// Implements password hashing and verification using ASP.NET Core Identity.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _identityPasswordHasher;

    /// <summary>
    /// Initializes a new instance of the <see cref="PasswordHasher"/> class.
    /// </summary>
    public PasswordHasher()
    {
        _identityPasswordHasher = new();
    }

    /// <summary>
    /// Hashes a password.
    /// </summary>
    /// <param name="password">The plaintext password to hash.</param>
    /// <returns>The hashed password.</returns>
    public string HashPassword(string password)
    {
        return _identityPasswordHasher.HashPassword(null!, password);
    }

    /// <summary>
    /// Verifies whether a provided password matches a hashed password.
    /// </summary>
    /// <param name="hashedPassword">The stored hashed password.</param>
    /// <param name="providedPassword">The plaintext password supplied by the user.</param>
    /// <returns><c>true</c> when the password matches; otherwise <c>false</c>.</returns>
    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        PasswordVerificationResult result = _identityPasswordHasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
