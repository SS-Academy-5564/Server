namespace Pulse.BL.Common.Security.Tokens;

public interface IRefreshTokenService
{
    string GenerateToken();
    string ComputeHash(string token);
}
