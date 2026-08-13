namespace Pulse.DAL.Commands.DashboardWidgets.UpdateWidget;

/// <summary>
/// The widget configuration to persist during an update.
/// </summary>
/// <param name="Id">The identifier of the widget to update.</param>
/// <param name="Type">The widget type identifier.</param>
/// <param name="Title">The widget title, or <c>null</c> to keep the current value.</param>
/// <param name="Subtitle">The widget subtitle, or <c>null</c> to keep the current value.</param>
/// <param name="Metric">The metric displayed by the widget.</param>
/// <param name="TimeRange">The time range covered by the widget.</param>
/// <param name="Settings">The widget settings, or <c>null</c> to keep the current value.</param>
/// <param name="OrganizationId">The organization that owns the widget.</param>
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
