namespace Pulse.BL.Features.DashboardWidgets.CreateWidget;

public record CreateWidgetCommand(
    Guid DashboardTabId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    DateTimeOffset TimeRange,
    string? Settings,
    Guid MonitorId
);
