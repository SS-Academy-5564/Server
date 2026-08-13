namespace Pulse.DAL.Queries.Monitors;

/// <summary>
/// Represents a request key for monitor metric statistics.
/// </summary>
/// <param name="MonitorId">The unique identifier of the monitor.</param>
/// <param name="Metric">The metric type to query.</param>
/// <param name="From">The start of the time range for the metric request.</param>
public record MonitorMetricRecord(
    Guid MonitorId,
    MetricType Metric,
    DateTimeOffset From);
