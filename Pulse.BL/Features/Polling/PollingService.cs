using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.Options;
using Pulse.DAL.Commands.MonitorPollResults;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.BL.Features.Polling;

public class PollingService : IPollingService
{
    private readonly ILogger<PollingService> _logger;
    private readonly PollingWorkerOptions _options;
    private readonly IMonitorQueries _monitorQueries;
    private readonly IMonitorCommands _monitorCommands;
    private readonly IMonitorPollResultsCommands _monitorPollResultCommands;
    private readonly IHttpMonitorClient _httpMonitorClient;
    private readonly IJsonPathReader _jsonPathReader;
    private readonly IUnitOfWorkFactory _unitOfWorkFactory;

    public PollingService(
        ILogger<PollingService> logger,
        IOptions<PollingWorkerOptions> options,
        IHttpMonitorClient httpMonitorClient,
        IJsonPathReader jsonPathReader,
        IMonitorQueries monitorQueries,
        IMonitorCommands monitorCommands,
        IMonitorPollResultsCommands monitorPollResultCommands,
        IUnitOfWorkFactory unitOfWorkFactory)
    {
        _logger = logger;
        _options = options.Value;
        _httpMonitorClient = httpMonitorClient;
        _jsonPathReader = jsonPathReader;
        _monitorQueries = monitorQueries;
        _monitorCommands = monitorCommands;
        _unitOfWorkFactory = unitOfWorkFactory;
        _monitorPollResultCommands = monitorPollResultCommands;
    }

    public async Task<Result> ProcessDueMonitorsAsync(CancellationToken ct = default)
    {
        IEnumerable<MonitorRecord> monitors = await _monitorQueries.GetDueEnabledAsync(_options.BatchSize, ct);

        foreach (MonitorRecord monitor in monitors)
        {
            try
            {
                ct.ThrowIfCancellationRequested();

                CreateMonitorPollResultsInput monitorPollResults = await GetPollResultAsync(monitor, ct);
                await SavePollResultAsync(monitor, monitorPollResults, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Failed to process monitor. MonitorId: {MonitorId}",
                    monitor.Id);
            }
        }

        return Result.Ok();
    }

    private async Task<CreateMonitorPollResultsInput> GetPollResultAsync(MonitorRecord monitor, CancellationToken ct)
    {
        HttpMonitorResponse response = await _httpMonitorClient.SendAsync(monitor, ct);
        string? value = null;

        if (response.IsSuccess && !string.IsNullOrWhiteSpace(response.Body))
        {
             value = _jsonPathReader.ReadValue(response.Body, monitor.ResultPath);
        }

        return new(
            Value: value,
            CheckedAt: DateTime.UtcNow,
            response.IsSuccess,
            response.ResponseTimeMs,
            response.StatusCode,
            monitor.Id,
            response.RequestStatus);
    }

    private async Task SavePollResultAsync(MonitorRecord monitor, CreateMonitorPollResultsInput resultInput, CancellationToken ct)
    {
        DateTime completedAt = DateTime.UtcNow;
        DateTime nextExecutionAt = completedAt.AddSeconds(monitor.PollingIntervalSeconds);

        UpdateMonitorAfterPollInput monitorInput = new(monitor.Id, resultInput.Value, completedAt, nextExecutionAt);

        await using IUnitOfWork uof = await _unitOfWorkFactory.CreateAsync(ct: ct);
        IDbSession session = (IDbSession)uof;
        await _monitorPollResultCommands.CreateAsync(resultInput, session, ct);
        await _monitorCommands.UpdateAfterPollAsync(monitorInput, session, ct);
        await uof.CommitAsync(ct);
    }
}
