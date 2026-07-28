namespace Pulse.API.Constants;

/// <summary>
/// Provides shared error-code constants for rate-limit responses.
/// </summary>
public static class RateLimitErrorCodes
{
    /// <summary>
    /// Gets the error code used when a request is rejected due to rate limiting.
    /// </summary>
    public const string RateLimited = "RateLimited";
}
