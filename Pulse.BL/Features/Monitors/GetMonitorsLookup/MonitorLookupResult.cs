namespace Pulse.BL.Features.Monitors.GetMonitorsLookup;

/// <summary>
/// Represents a monitor lookup result item.
/// </summary>
/// <param name="Name">The display name of the monitor.</param>
/// <param name="Id">The unique identifier of the monitor.</param>
public record MonitorLookupResult(string Name, Guid Id);
