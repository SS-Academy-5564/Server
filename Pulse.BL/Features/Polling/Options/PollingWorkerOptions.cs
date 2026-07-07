namespace Pulse.BL.Features.Polling;

/// <summary>
/// Defines options for Polling worker configuration
/// </summary>
public class PollingWorkerOptions
{
    public const string SectionName = "PollingWorker";

    /// <summary>
    /// Defines how often the worker checks for due monitors.
    /// Must be between 1 and 60 seconds.
    /// </summary>
    public int LoopIntervalSeconds { get; init; }

    /// <summary>
    /// Limits the number of monitors selected per worker iteration.
    /// </summary>
    public int BatchSize { get; init; }

    /// <summary>
    /// Limits the number of parallel HTTP requests
    /// </summary>
    public int MaxConcurrentRequests { get; init; }
}
