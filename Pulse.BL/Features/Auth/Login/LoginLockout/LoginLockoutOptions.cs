namespace Pulse.BL.Features.Auth.Login.LoginLockout;

/// <summary>
/// Defines account lockout options for failed login attempts.
/// </summary>
public sealed class LoginLockoutOptions
{
    /// <summary>
    ///  Configuration section for login Lockout.
    /// </summary>
    public const string SectionName = "Authentication:LoginLockout";

    /// <summary>
    /// Gets the maximum number of failed login attempts before lockout.
    /// </summary>
    public int MaxFailedAttempts { get; init; }

    /// <summary>
    /// Gets the lockout duration, in minutes.
    /// /// </summary>
    public int LockoutDurationMinutes { get; init; }
}
