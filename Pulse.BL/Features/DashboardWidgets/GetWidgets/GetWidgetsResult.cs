namespace Pulse.BL.Features.DashboardWidgets.GetWidgets;

public record GetWidgetsResult(
    Guid Id,
    Guid DashboardTabId,
    string Type,
    string Metric,
    string TimeRange,
    string? Settings
);
