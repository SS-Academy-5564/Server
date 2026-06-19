namespace Pulse.BL.Common.Security.Tokens;

public sealed record GeneratedJwtToken(string Token, DateTimeOffset ExpiresAt);

public interface IJwtTokenGenerator
{
    GeneratedJwtToken GenerateToken(Guid userId, string roleName, Guid organizationId);
}
