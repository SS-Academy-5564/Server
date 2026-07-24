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
        IEnumerable<MonitorPollingRecord> monitors = await _monitorQueries.GetDueEnabledAsync(_options.BatchSize, ct);

        foreach (MonitorPollingRecord monitor in monitors)
        {
            try
            {
                ct.ThrowIfCancellationRequested();
                await ProcessMonitorAsync(monitor, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to process monitor. MonitorId: {MonitorId}", monitor.Id);
            }
        }

        return Result.Ok();
    }

    public async Task<Result> ProcessMonitorAsync(MonitorPollingRecord monitor, CancellationToken ct)
    {
        try
        {
            CreateMonitorPollResultsInput monitorPollResults = await GetPollResultAsync(monitor, ct);
            await SavePollResultAsync(monitor, monitorPollResults, ct);
            return Result.Ok();
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process monitor. MonitorId: {MonitorId}", monitor.Id);
            return Result.Fail("Failed to process monitor.");
        }
    }

    private async Task<CreateMonitorPollResultsInput> GetPollResultAsync(MonitorPollingRecord monitor, CancellationToken ct)
    {
        HttpMonitorResponse response = await _httpMonitorClient.SendAsync(monitor, ct);
        bool isSuccess = response.IsSuccess;
        string requestStatus = response.RequestStatus;
        string? value = null;

        if (isSuccess)
        {
            bool extractionSucceeded =
                !string.IsNullOrWhiteSpace(response.Body) &&
                _jsonPathReader.TryReadValue(response.Body, monitor.ResultPath, out value) &&
                value is not null;

            if (!extractionSucceeded)
            {
                _logger.LogWarning(
                    "Failed to extract monitor value. MonitorId: {MonitorId}, ResultPath: {ResultPath}",
                    monitor.Id,
                    monitor.ResultPath);

                isSuccess = false;
                requestStatus = RequestStatusNames.ExtractionError;
            }
        }

        return new CreateMonitorPollResultsInput(
            Value: value,
            CheckedAt: DateTime.UtcNow,
            IsSuccess: isSuccess,
            ResponseTimeMs: response.ResponseTimeMs,
            StatusCode: response.StatusCode,
            MonitorId: monitor.Id,
            RequestStatus: requestStatus);
    }

    private async Task SavePollResultAsync(MonitorPollingRecord monitor, CreateMonitorPollResultsInput resultInput, CancellationToken ct)
    {
        DateTime completedAt = DateTime.UtcNow;
        DateTime nextExecutionAt = completedAt.AddSeconds(monitor.PollingIntervalSeconds);

        string status = resultInput.IsSuccess
            ? nameof(MonitorStatus.Enabled)
            : nameof(MonitorStatus.Error);

        UpdateMonitorAfterPollInput monitorInput = new(monitor.Id, resultInput.Value, completedAt, nextExecutionAt, status);

        await using IUnitOfWork uof = await _unitOfWorkFactory.CreateAsync(ct: ct);
        IDbSession session = (IDbSession)uof;

        await _monitorPollResultCommands.CreateAsync(resultInput, session, ct);
        await _monitorCommands.UpdateAfterPollAsync(monitorInput, session, ct);

        await uof.CommitAsync(ct);
    }
}
