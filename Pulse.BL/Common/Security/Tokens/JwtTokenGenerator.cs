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

    /// <inheritdoc/>
    public JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        _options = options.Value;
        _timeProvider = timeProvider;

        ArgumentException.ThrowIfNullOrWhiteSpace(_options.SecretKey);

        _secretKeyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
    }

    /// <inheritdoc/>
    public GeneratedJwtToken GenerateToken(Guid userId, string roleName, Guid organizationId)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();
        DateTimeOffset expiresAt = now.AddMinutes(_options.ExpirationMinutes);

        Claim[] claims =
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtClaimNames.Role, roleName),
            new(JwtClaimNames.OrganizationId, organizationId.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        SymmetricSecurityKey key = new(_secretKeyBytes);
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            NotBefore = now.UtcDateTime,
            Expires = expiresAt.UtcDateTime,
            SigningCredentials = credentials
        };

        return new GeneratedJwtToken(
            TokenHandler.CreateToken(tokenDescriptor),
            expiresAt
        );
    }
}
