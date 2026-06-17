using System.Security.Cryptography;
using System.Text;

namespace Pulse.BL.Common.Security;

public static class PiiHasher
{
    private const int HashPrefixLength = 16;

    public static string HashForLogging(string value)
    {
        var normalized = value.Trim().ToLowerInvariant();
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(hash)[..HashPrefixLength];
    }
}
