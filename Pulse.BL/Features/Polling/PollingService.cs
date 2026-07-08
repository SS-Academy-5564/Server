using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.Json;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Commands.MonitorPollResults;
using Pulse.DAL.Commands.Monitors;
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
        var monitors = await _monitorQueries.GetDueEnabledAsync(_options.BatchSize, ct);

        ParallelOptions options = new()
        {
            MaxDegreeOfParallelism = _options.MaxConcurrentRequests,
            CancellationToken = ct
        };

        await Parallel.ForEachAsync(monitors, options, ProcessMonitorAsync);

        return Result.Ok();
    }

    private async ValueTask ProcessMonitorAsync(MonitorRecord monitor, CancellationToken ct)
    {
        DateTime checkedAt = DateTime.UtcNow;
        string? value = null;
        CreateMonitorPollResultsInput resultInput;

        try
        {
            HttpMonitorResponse response = await _httpMonitorClient.SendAsync(monitor, ct);
            value = TryReadMonitorValue(response, monitor);
            resultInput = CreatePollResultInput(monitor, response, value, checkedAt);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected monitor polling error. MonitorId: {MonitorId}", monitor.Id);

            resultInput = new CreateMonitorPollResultsInput(
                Value: null,
                CheckedAt: checkedAt,
                IsSuccess: false,
                ResponseTimeMs: 0,
                StatusCode: null,
                ErrorMessage: exception.Message,
                MonitorId: monitor.Id,
                RequestStatus: RequestStatusNames.UnexpectedError);
        }

        await PersistPollResultAsync(monitor, resultInput, value, checkedAt, ct);
    }

    private string? TryReadMonitorValue(HttpMonitorResponse response, MonitorRecord monitor)
    {
        if (!response.IsSuccess || string.IsNullOrWhiteSpace(response.Body))
        {
            return null;
        }

        return _jsonPathReader.ReadValue(response.Body, monitor.ResultPath);
    }

    private static CreateMonitorPollResultsInput CreatePollResultInput(
        MonitorRecord monitor,
        HttpMonitorResponse response,
        string? value,
        DateTime checkedAt)
    {
        return new CreateMonitorPollResultsInput(
            value,
            checkedAt,
            response.IsSuccess,
            response.ResponseTimeMs,
            response.StatusCode,
            response.ErrorMessage,
            monitor.Id,
            response.RequestStatus);
    }

    private async Task PersistPollResultAsync(
        MonitorRecord monitor,
        CreateMonitorPollResultsInput resultInput,
        string? value,
        DateTime checkedAt,
        CancellationToken ct)
    {
        DateTime nextExecutionAt = checkedAt.AddSeconds(monitor.PollingIntervalSeconds);

        UpdateMonitorAfterPollInput monitorInput = new(
            monitor.Id,
            value,
            checkedAt,
            nextExecutionAt);

        await using IUnitOfWork uof = await _unitOfWorkFactory.CreateAsync(ct);
        await _monitorPollResultCommands.CreateAsync(resultInput, uof, ct);
        await _monitorCommands.UpdateAfterPollAsync(monitorInput, uof, ct);
        await uof.CommitAsync(ct);
    }
}
