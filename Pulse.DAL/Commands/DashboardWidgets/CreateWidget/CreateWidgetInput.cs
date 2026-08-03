namespace Pulse.DAL.Commands.DashboardWidgets.CreateWidget;

public record CreateWidgetInput(
    Guid DashboardTabId,
    string Type,
    string Metric,
    string TimeRange,
    string? Settings,
    Guid OrganizationId
);
