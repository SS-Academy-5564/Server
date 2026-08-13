namespace Pulse.DAL.Queries.Monitors;

/// <summary>
/// Represents a database lookup record for a monitor.
/// </summary>
/// <param name="Name">The name of the monitor.</param>
/// <param name="Id">The identifier of the monitor.</param>
public record MonitorLookupRecord(string Name, Guid Id);
