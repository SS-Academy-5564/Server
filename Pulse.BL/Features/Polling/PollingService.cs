using FluentResults;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling.Http;
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

            if (response.IsSuccess && string.IsNullOrWhiteSpace(response.Body))
            {
                value =  _jsonPathReader.ReadValue(response.Body, monitor.ResultPath);
            }


            resultInput = new (
                value,
                checkedAt,
                response.IsSuccess,
                response.ResponseTimeMs,
                response.StatusCode,
                response.ErrorMessage,
                monitor.Id,
                response.RequestStatus);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unexpected monitor polling error. MonitorId: {MonitorId}", monitor.Id);

            resultInput = new (
                Value: null,
                CheckedAt: checkedAt,
                IsSuccess: false,
                ResponseTimeMs: 0,
                StatusCode: null,
                ErrorMessage: exception.Message,
                MonitorId: monitor.Id,
                RequestStatus: RequestStatusNames.UnexpectedError);
        }

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
