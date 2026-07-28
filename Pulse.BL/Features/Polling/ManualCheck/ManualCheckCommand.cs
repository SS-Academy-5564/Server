namespace Pulse.BL.Features.Polling.ManualCheck;

/// <summary>
/// Represents a request to queue a monitor for a manual check.
/// </summary>
public sealed record ManualCheckCommand
{
    /// <summary>
    /// Gets the identifier of the monitor to check manually.
    /// </summary>
    public Guid MonitorId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualCheckCommand"/> record.
    /// </summary>
    /// <param name="monitorId">The identifier of the monitor to queue for processing.</param>
    public ManualCheckCommand(Guid monitorId)
    {
        MonitorId = monitorId;
    }
}
