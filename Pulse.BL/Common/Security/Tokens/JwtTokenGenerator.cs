using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Pulse.BL.Common.Security.Tokens;

/// <summary>
/// Generates JSON Web Tokens using the configured JWT options.
/// </summary>
public class JwtTokenGenerator : IJwtTokenGenerator
{
    private static readonly JsonWebTokenHandler TokenHandler = new();

    private readonly JwtOptions _options;
    private readonly byte[] _secretKeyBytes;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtTokenGenerator"/> class.
    /// </summary>
    /// <param name="options">The JWT options used to generate tokens.</param>
    /// <param name="timeProvider">The time provider used to calculate expiration.</param>
    public JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _options = options.Value;
        _timeProvider = timeProvider;
        _secretKeyBytes = Encoding.UTF8.GetBytes(_options.SecretKey);
    }

    /// <summary>
    /// Generates a JWT token for the specified user context.
    /// </summary>
    /// <param name="userId">The identifier of the user.</param>
    /// <param name="roleName">The role assigned to the user.</param>
    /// <param name="organizationId">The identifier of the organization.</param>
    /// <returns>A generated JWT token along with its expiration time.</returns>
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

        return new GeneratedJwtToken(TokenHandler.CreateToken(tokenDescriptor), expiresAt);
    }
}
