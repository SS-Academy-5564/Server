namespace Pulse.BL.Common.Security.Tokens;

/// <summary>
/// Configuration options for refresh tokens.
/// </summary>
public class RefreshTokenOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "RefreshToken";

    /// <summary>
    /// Gets or sets the number of days until a refresh token expires.
    /// </summary>
    public int ExpirationDays { get; set; } = 14;
}
