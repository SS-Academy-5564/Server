namespace Pulse.BL.Features.Monitors;

public sealed record GetMonitorsQuery(MonitorStatus? Status, int? PageNumber, int? PageSize);

