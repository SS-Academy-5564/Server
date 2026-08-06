namespace Pulse.BL.Features.Auth.EmailVerification;

/// <summary>
/// Configures email verification token lifetime and the client verification page.
/// </summary>
public sealed class EmailVerificationOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "EmailVerification";

    /// <summary>
    /// Gets or sets the number of hours for which a verification token remains valid.
    /// </summary>
    public int TokenLifetimeHours { get; set; }

    /// <summary>
    /// Gets or sets the minimum number of seconds between replacement verification emails.
    /// </summary>
    public int ResendCooldownSeconds { get; set; }

    /// <summary>
    /// Gets or sets the absolute client URL that receives the verification token.
    /// </summary>
    public string VerificationPageUrl { get; set; } = string.Empty;
}
