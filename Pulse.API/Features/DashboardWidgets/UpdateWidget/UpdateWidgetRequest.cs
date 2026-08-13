namespace Pulse.API.Features.DashboardWidgets.UpdateWidget;

/// <summary>
/// The request to update the configuration of an existing widget.
/// </summary>
/// <param name="Type">The widget type identifier.</param>
/// <param name="Title">The widget title, or <c>null</c> to keep the current value.</param>
/// <param name="Subtitle">The widget subtitle, or <c>null</c> to keep the current value.</param>
/// <param name="Metric">The metric displayed by the widget.</param>
/// <param name="TimeRange">The time range covered by the widget.</param>
/// <param name="Settings">The widget settings, or <c>null</c> to keep the current value.</param>
public record UpdateWidgetRequest(
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    string TimeRange,
    string? Settings
);
