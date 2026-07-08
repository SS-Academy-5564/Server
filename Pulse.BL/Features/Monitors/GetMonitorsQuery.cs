using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Monitors;

public sealed record GetMonitorsQuery(MonitorStatus? Status);
