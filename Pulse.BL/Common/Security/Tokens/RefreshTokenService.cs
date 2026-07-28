using System.Security.Cryptography;
using System.Text;

namespace Pulse.BL.Common.Security.Tokens;

public class RefreshTokenService : IRefreshTokenService
{
    public string GenerateToken()
    {
        byte[] randomBytes = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }

        return Convert.ToBase64String(randomBytes);
    }

    public string ComputeHash(string token)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(token);
        byte[] hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
