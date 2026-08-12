namespace Pulse.API.Features.DashboardWidgets.CreateWidget;

public record CreateWidgetRequest(
    Guid DashboardTabId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    DateTimeOffset TimeRange,
    string? Settings,
    Guid MonitorId
);
