namespace Pulse.BL.Features.Auth.PasswordReset;

/// <summary>
/// Options for password reset security constraints.
/// </summary>
public sealed class PasswordResetOptions
{
    public const string SectionName = "PasswordReset";

    /// <summary>
    /// Gets or sets the time-to-live for a password reset OTP code in minutes.
    /// </summary>
    public required int CodeTtlMinutes { get; init; } = 5;

    /// <summary>
    /// Gets or sets the maximum number of failed verification attempts before the code is invalidated.
    /// </summary>
    public required int MaxFailedAttempts { get; init; } = 5;

    /// <summary>
    /// Gets or sets the lifetime of the JWT reset token in minutes.
    /// </summary>
    public required int ResetTokenLifetimeMinutes { get; init; } = 10;

    /// <summary>
    /// Gets or sets the minimum interval in seconds between successive code resend requests for the same user.
    /// </summary>
    public required int ResendCooldownSeconds { get; init; } = 60;
}
