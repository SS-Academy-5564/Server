namespace Pulse.API.Constants;

/// <summary>
/// Defines the names of the rate limiting policies used across the API.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// The policy used to rate limit login attempts.
    /// </summary>
    public const string Login = "LoginRateLimit";

    /// <summary>
    /// The policy used to rate limit password reset attempts.
    /// </summary>
    public const string PasswordReset = "PasswordResetLimit";

    /// <summary>
    /// The policy used to rate limit registration attempts.
    /// </summary>
    public const string Registration = "RegistrationRateLimit";

    /// <summary>
    /// The policy used to rate limit manual monitor triggers.
    /// </summary>
    public const string ManualMonitorTrigger = "ManualMonitorTrigger";

    /// <summary>
    /// The policy used to rate limit token refresh attempts.
    /// </summary>
    public const string Refresh = "RefreshRateLimit";
}
