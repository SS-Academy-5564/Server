namespace Pulse.BL.Common.Security.Tokens;

public interface IJwtTokenGenerator
{
    string GenerateToken(Guid userId, Guid roleId, Guid organizationId, out DateTimeOffset expiresAt);
}
