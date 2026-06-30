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

    /// <inheritdoc/>
    public string GeneratePasswordResetToken(Guid userId, string jti, TimeSpan expiresIn)
    {
        DateTimeOffset now = _timeProvider.GetUtcNow();

        Claim[] claims =
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtClaimNames.Purpose, JwtClaimNames.PasswordResetPurpose),
            new(JwtRegisteredClaimNames.Jti, jti)
        };

        SymmetricSecurityKey key = new(_secretKeyBytes);
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            NotBefore = now.UtcDateTime,
            Expires = now.Add(expiresIn).UtcDateTime,
            SigningCredentials = credentials
        };

        return TokenHandler.CreateToken(tokenDescriptor);
    }

    /// <inheritdoc/>
    public (Guid UserId, string Jti)? ValidatePasswordResetToken(string token)
    {
        SymmetricSecurityKey key = new(_secretKeyBytes);

        TokenValidationParameters validationParameters = new()
        {
            ValidateIssuer = true,
            ValidIssuer = _options.Issuer,
            ValidateAudience = true,
            ValidAudience = _options.Audience,
            ValidateLifetime = true,
            IssuerSigningKey = key,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            TokenValidationResult result = TokenHandler.ValidateTokenAsync(token, validationParameters).GetAwaiter().GetResult();

            if (!result.IsValid)
            {
                return null;
            }

            string? purpose = result.ClaimsIdentity.FindFirst(JwtClaimNames.Purpose)?.Value;
            if (purpose != JwtClaimNames.PasswordResetPurpose)
            {
                return null;
            }

            string? sub = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            string? jti = result.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            
            if (Guid.TryParse(sub, out Guid userId) && !string.IsNullOrEmpty(jti))
            {
                return (userId, jti);
            }
            
            return null;
        }
        catch
        {
            return null;
        }
    }
}
