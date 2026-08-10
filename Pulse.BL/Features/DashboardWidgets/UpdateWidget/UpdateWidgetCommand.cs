namespace Pulse.BL.Features.DashboardWidgets.UpdateWidget;

/// <summary>
/// The command to update the configuration of an existing widget.
/// </summary>
public record UpdateWidgetCommand(
    Guid WidgetId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    string TimeRange,
    string? Settings
);
