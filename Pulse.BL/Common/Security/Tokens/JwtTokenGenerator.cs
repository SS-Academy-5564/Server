using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Pulse.BL.Common.Security.Tokens;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private static readonly JsonWebTokenHandler TokenHandler = new();

    private readonly JwtOptions _options;
    private readonly byte[] _secretKeyBytes;
    private readonly TimeProvider _timeProvider;

    public JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _options = options.Value;
        _timeProvider = timeProvider;
        _secretKeyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
    }

    public GeneratedJwtToken GenerateToken(Guid userId, string roleName, Guid organizationId)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.AddMinutes(_options.ExpirationMinutes);

        Claim[] claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtClaimNames.Role, roleName),
            new Claim(JwtClaimNames.OrganizationId, organizationId.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(_secretKeyBytes);
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = credentials
        };

        return new GeneratedJwtToken(TokenHandler.CreateToken(tokenDescriptor), expiresAt);
    }
}
