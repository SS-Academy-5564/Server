namespace Pulse.BL.Features.Monitors;

public sealed record GetMonitorsQuery(MonitorStatus? Status, long? PageNumber, int? PageSize);

