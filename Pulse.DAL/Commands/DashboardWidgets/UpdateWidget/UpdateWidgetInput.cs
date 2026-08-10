namespace Pulse.DAL.Commands.DashboardWidgets.UpdateWidget;

/// <summary>
/// The widget configuration to persist during an update.
/// </summary>
public record UpdateWidgetInput(
    Guid Id,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    string TimeRange,
    string? Settings,
    Guid OrganizationId
);
