namespace Pulse.API.Features.DashboardWidgets.CreateWidget;

public record CreateWidgetRequest(
    Guid DashboardTabId,
    string Type,
    string Metric,
    string TimeRange,
    string? Settings
);
