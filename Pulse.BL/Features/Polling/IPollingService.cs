using FluentResults;

namespace Pulse.BL.Features.Polling;

public interface IPollingService
{
    Task<Result> ProcessDueMonitorsAsync(CancellationToken stoppingToken);
}
