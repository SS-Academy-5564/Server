using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling;

public class PollingService : IPollingService
{
    private readonly ILogger<PollingWorkerOptionsValidator> _logger;
    private readonly PollingWorkerOptions _options;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMonitorQueries _monitorQueries;
    private readonly IMonitorCommands _monitorCommands;

    public PollingService(
        ILogger<PollingWorkerOptionsValidator> logger,
        IOptions<PollingWorkerOptions>  options,
        IHttpClientFactory httpClientFactory,
        IMonitorQueries monitorQueries,
        IMonitorCommands monitorCommands)
    {
        _logger= logger;
        _options = options.Value;
        _httpClientFactory = httpClientFactory;
        _monitorQueries = monitorQueries;
        _monitorCommands = monitorCommands;
    }

    public async Task<Result> ProcessDueMonitorsAsync(CancellationToken ct = default)
    {
        var monitors = await _monitorQueries.GetDueEnabledAsync(_options.BatchSize);
        ParallelOptions options = new() { MaxDegreeOfParallelism = _options.MaxConcurrentRequests};

        Parallel.ForEachAsync(monitors, options, async (monitors, ct) =>
        {
            var httpClient = _httpClientFactory.CreateClient();

            HttpResponseMessage responseMessage;
            // Get response result

            // uof
            // save
            // update
        });
        return Result.Ok();
    }
}
