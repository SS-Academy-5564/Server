namespace Pulse.API.Common.Security.RateLimiting;

/// <summary>
/// Configuration section names for rate limiting rules.
/// </summary>
public static class RateLimitSections
{
    /// <summary>
    /// Configuration section for login rate limiting.
    /// </summary>
    public const string Login = "RateLimit:Login";

    /// <summary>
    /// Configuration section for password reset request rate limiting.
    /// </summary>
    public const string PasswordReset = "RateLimit:PasswordReset";
}
