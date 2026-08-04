using System.Security.Cryptography;
using System.Text;

namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Generates 256-bit email verification tokens and hashes them with SHA-256.
/// </summary>
public sealed class EmailVerificationTokenService : IEmailVerificationTokenService
{
    private const int TokenSizeBytes = 32;

    /// <inheritdoc/>
    public string GenerateToken()
    {
        byte[] tokenBytes = RandomNumberGenerator.GetBytes(TokenSizeBytes);

        return Convert.ToBase64String(tokenBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    /// <inheritdoc/>
    public string ComputeHash(string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
    }
}
