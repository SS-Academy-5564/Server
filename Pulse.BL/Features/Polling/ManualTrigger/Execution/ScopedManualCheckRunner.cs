using Microsoft.Extensions.DependencyInjection;

namespace Pulse.BL.Features.Polling.ManualTrigger.Execution;

public sealed class ScopedManualCheckRunner : IScopedManualCheckRunner
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ScopedManualCheckRunner(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task RunAsync(Guid monitorId, CancellationToken ct)
    {
        using IServiceScope scope = _scopeFactory.CreateScope();
        IManualCheckExecutor executor = scope.ServiceProvider.GetRequiredService<IManualCheckExecutor>();
        await executor.ExecuteAsync(monitorId, ct);
    }
}
