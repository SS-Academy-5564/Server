namespace Pulse.BL.Features.DashboardWidgets.GetWidgets;

public record GetWidgetsResult(
    Guid Id,
    Guid DashboardTabId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    string TimeRange,
    string? Settings,
    decimal? Value
);
