using Pulse.DAL.Queries.Monitors;

namespace Pulse.API.Features.Monitors.GetMonitors;

public sealed record GetMonitorsRequest(MonitorStatus? Status);
