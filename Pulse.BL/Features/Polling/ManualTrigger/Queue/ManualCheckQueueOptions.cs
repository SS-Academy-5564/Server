namespace Pulse.BL.Features.Polling.ManualTrigger.Queue;

/// <summary>
/// Configuration for the bounded queue of manually-triggered monitor checks.
/// </summary>
public sealed class ManualCheckQueueOptions
{
    public const string SectionName = "ManualCheckQueue";

    /// <summary>
    /// Maximum number of manual checks that may be queued at once.
    /// </summary>
    public int Capacity { get; init; } = 100;
}
