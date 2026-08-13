namespace Pulse.BL.Features.DashboardWidgets.CreateWidget;

/// <summary>
/// Command to create a new dashboard widget.
/// </summary>
/// <param name="DashboardTabId">The identifier of the dashboard tab where the widget is created.</param>
/// <param name="Type">The widget type.</param>
/// <param name="Title">The optional title of the widget.</param>
/// <param name="Subtitle">The optional subtitle of the widget.</param>
/// <param name="Metric">The metric displayed by the widget.</param>
/// <param name="TimeRange">The start time range for the widget data.</param>
/// <param name="Settings">The optional JSON settings configuration for the widget.</param>
/// <param name="MonitorId">The identifier of the monitor associated with the widget.</param>
public record CreateWidgetCommand(
    Guid DashboardTabId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    DateTimeOffset TimeRange,
    string? Settings,
    Guid MonitorId
);
