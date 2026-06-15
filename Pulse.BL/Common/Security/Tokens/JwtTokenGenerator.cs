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
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));

        if (string.IsNullOrWhiteSpace(_options.SecretKey))
            throw new InvalidOperationException("JWT SecretKey must be configured.");

        _secretKeyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
    }

    public string GenerateToken(Guid userId, Guid roleId, Guid organizationId, out DateTimeOffset expiresAt)
    {
        var now = _timeProvider.GetUtcNow();
        expiresAt = now.AddMinutes(_options.ExpirationMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim("roleId", roleId.ToString()),
            new Claim("orgId", organizationId.ToString()),
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

        return TokenHandler.CreateToken(tokenDescriptor);
    }
}
