namespace Pulse.DAL.Queries.DashboardWidgets.GetWidgets;

public record WidgetQueryResult(
    Guid Id,
    Guid DashboardTabId,
    string Type,
    string Metric,
    string TimeRange,
    string? Settings
);

