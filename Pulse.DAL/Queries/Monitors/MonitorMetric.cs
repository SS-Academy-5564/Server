namespace Pulse.DAL.Queries.Monitors;

/// <summary>
/// Pairs a monitor with the metric and time window to retrieve, forming a self-contained statistics request.
/// </summary>
/// <param name="MonitorId">The monitor to retrieve data for.</param>
/// <param name="Metric">The metric type to project from poll results.</param>
/// <param name="From">The inclusive lower bound of the time window.</param>
public sealed record MonitorMetric(Guid MonitorId, MetricType Metric, DateTimeOffset From)
{
    /// <summary>
    /// Creates a <see cref="MonitorMetric"/> by parsing the metric name stored in a widget row.
    /// Unrecognised names fall back to <see cref="MetricType.ResponseTime"/>.
    /// </summary>
    /// <param name="monitorId">The monitor identifier.</param>
    /// <param name="metricName">The raw metric string as stored in the widget (case-insensitive).</param>
    /// <param name="from">The inclusive lower bound of the time window.</param>
    /// <returns>A <see cref="MonitorMetric"/> ready to pass to the statistics query.</returns>
    public static MonitorMetric FromWidget(Guid monitorId, string metricName, DateTimeOffset from) =>
        new(monitorId, ParseMetricName(metricName), from);

    private static MetricType ParseMetricName(string name) =>
        name.ToLowerInvariant() switch
        {
            "availability" => MetricType.Availability,
            "requests"     => MetricType.Requests,
            "errors"       => MetricType.Errors,
            _              => MetricType.ResponseTime,
        };
}
