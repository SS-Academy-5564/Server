namespace Pulse.DAL.Queries.DashboardWidgets.GetWidgets;

/// <summary>
/// Represents a database query result row for a dashboard widget.
/// </summary>
/// <param name="Id">The unique identifier of the widget.</param>
/// <param name="DashboardTabId">The identifier of the dashboard tab containing the widget.</param>
/// <param name="MonitorId">The identifier of the associated monitor.</param>
/// <param name="Type">The widget type.</param>
/// <param name="Title">The optional title of the widget.</param>
/// <param name="Subtitle">The optional subtitle of the widget.</param>
/// <param name="Metric">The metric displayed by the widget.</param>
/// <param name="TimeRange">The start time range for the widget data.</param>
/// <param name="Settings">The optional JSON settings configuration for the widget.</param>
public record WidgetQueryResult(
    Guid Id,
    Guid DashboardTabId,
    Guid MonitorId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    DateTimeOffset TimeRange,
    string? Settings
);
