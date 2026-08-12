namespace Pulse.DAL.Commands.DashboardWidgets.CreateWidget;

public record CreateWidgetInput(
    Guid DashboardTabId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    string TimeRange,
    string? Settings,
    Guid MonitorId,
    Guid OrganizationId
);
