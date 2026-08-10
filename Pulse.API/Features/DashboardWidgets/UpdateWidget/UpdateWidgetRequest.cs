namespace Pulse.API.Features.DashboardWidgets.UpdateWidget;

/// <summary>
/// The request to update the configuration of an existing widget.
/// </summary>
public record UpdateWidgetRequest(
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    string TimeRange,
    string? Settings
);
