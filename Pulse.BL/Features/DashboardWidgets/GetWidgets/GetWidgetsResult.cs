namespace Pulse.BL.Features.DashboardWidgets.GetWidgets;

/// <summary>
/// Represents the result of fetching a dashboard widget, including its metadata and metric values.
/// </summary>
/// <param name="Id">The unique identifier of the widget.</param>
/// <param name="DashboardTabId">The dashboard tab identifier to which this widget belongs.</param>
/// <param name="Type">The widget type.</param>
/// <param name="Title">The optional widget title.</param>
/// <param name="Subtitle">The optional widget subtitle.</param>
/// <param name="Metric">The metric calculated or displayed by the widget.</param>
/// <param name="TimeRange">The start-of-range timestamp for the widget as provided by the client.</param>
/// <param name="Settings">The optional JSON settings string for the widget.</param>
/// <param name="Value">The collection of metric values for the widget.</param>
public record GetWidgetsResult(
    Guid Id,
    Guid DashboardTabId,
    string Type,
    string? Title,
    string? Subtitle,
    string Metric,
    DateTimeOffset TimeRange,
    string? Settings,
    IEnumerable<decimal> Value
);
