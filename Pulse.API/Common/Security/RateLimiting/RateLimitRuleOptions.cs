namespace Pulse.API.Common.Security.RateLimiting;

/// <summary>
/// Defines options for a rate limiting rule.
/// </summary>
public sealed class RateLimitRuleOptions
{
    /// <summary>
    /// Gets or sets the maximum number of allowed attempts.
    /// </summary>
    public int MaxAttempts { get; set; }

    /// <summary>
    /// Gets or sets the period, in minutes, used to replenish attempts.
    /// </summary>
    public int PeriodMinutes { get; set; }
}
