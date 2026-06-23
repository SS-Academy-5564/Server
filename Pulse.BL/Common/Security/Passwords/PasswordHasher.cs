using Microsoft.AspNetCore.Identity;

namespace Pulse.BL.Common.Security.Passwords;

public class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<object> _identityPasswordHasher;

    public PasswordHasher()
    {
        _identityPasswordHasher = new();
    }

    /// <inheritdoc/>
    public string HashPassword(string password)
    {
        return _identityPasswordHasher.HashPassword(null!, password);
    }

    /// <inheritdoc/>
    public bool VerifyHashedPassword(string hashedPassword, string providedPassword)
    {
        PasswordVerificationResult result = _identityPasswordHasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}