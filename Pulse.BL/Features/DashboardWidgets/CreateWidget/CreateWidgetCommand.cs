namespace Pulse.BL.Features.DashboardWidgets.CreateWidget;

public record CreateWidgetCommand(
    Guid DashboardTabId,
    string Type,
    string Metric,
    string TimeRange,
    string? Settings
);
