namespace Pulse.API.Common.Security.RateLimiting;

/// <summary>
/// Defines options for a sliding window rate limiting rule.
/// </summary>
public sealed class SlidingWindowRateLimitRuleOptions : RateLimitRuleOptions
{
    /// <summary>
    /// Gets or sets the number of segments used for rate limiting.
    /// </summary>
    public int Segments { get; set; } = 1;
}
