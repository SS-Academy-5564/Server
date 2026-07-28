namespace Pulse.BL.Features.Polling.UpdateNotifier;

public interface IMonitorUpdateNotifier
{
    Task NotifyAsync(MonitorUpdate update, CancellationToken ct);
    Task NotifyAsync(List<MonitorUpdate> update, CancellationToken ct);
}

public class MonitorUpdate
{
}
