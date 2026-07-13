using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling.Http;

public interface IHttpMonitorClient
{
    Task<HttpMonitorResponse> SendAsync(MonitorRecord monitor, CancellationToken ct);
}
