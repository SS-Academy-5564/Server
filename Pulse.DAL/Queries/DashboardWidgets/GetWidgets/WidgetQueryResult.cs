namespace Pulse.DAL.Queries.DashboardWidgets.GetWidgets;

public record WidgetQueryResult(
    Guid Id,
    Guid DashboardTabId,
    Guid MonitorId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    string TimeRange,
    string? Settings
);
