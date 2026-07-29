namespace Pulse.BL.Features.Monitors;

/// <summary>
/// Represents a query for retrieving a filtered page of monitors.
/// </summary>
/// <param name="Status">The monitor status to filter by, or <c>null</c> to include all statuses.</param>
/// <param name="PageNumber">The one-based page number, or <c>null</c> to use the default.</param>
/// <param name="PageSize">The page size, or <c>null</c> to use the default.</param>
public sealed record GetMonitorsQuery(MonitorStatus? Status, int? PageNumber, int? PageSize)
{
    /// <summary>
    /// Gets or sets the text used to filter monitors by name.
    /// </summary>
    public string? SearchString { get; set; }
};
