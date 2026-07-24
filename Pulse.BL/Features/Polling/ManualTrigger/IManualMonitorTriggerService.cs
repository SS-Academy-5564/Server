using FluentResults;

namespace Pulse.BL.Features.Polling.ManualTrigger;

public interface IManualMonitorTriggerService
{
    Task<Result> TriggerAsync(Guid monitorId, CancellationToken ct);
}
