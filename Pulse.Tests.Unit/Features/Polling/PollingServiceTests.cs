using System.Data;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.Options;
using Pulse.DAL.Commands.MonitorPollResults;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Polling;

public class PollingServiceTests
{
    private readonly Mock<IHttpMonitorClient> _httpMonitorClient = new();
    private readonly Mock<IJsonPathReader> _jsonPathReader = new();
    private readonly Mock<IMonitorCommands> _monitorCommands = new();
    private readonly Mock<IMonitorPollResultsCommands> _monitorPollResultsCommands = new();
    private readonly Mock<IMonitorQueries> _monitorQueries = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IUnitOfWorkFactory> _unitOfWorkFactory = new();
    private readonly DAL.Queries.Monitors.MonitorPollingRecord _monitor = new(
        Guid.NewGuid(),
        "https://example.com/health",
        "GET",
        "data.status",
        60,
        30);
    private readonly PollingService _service;
    private CreateMonitorPollResultsInput? _createdMonitorPollResults;
    private UpdateMonitorAfterPollInput? _updatedMonitor;
    private IDbSession? _createdMonitorPollResultsSession;
    private IDbSession? _updatedMonitorSession;

    public PollingServiceTests()
    {
        _unitOfWork.As<IDbSession>();

        _monitorPollResultsCommands
            .Setup(c => c.CreateAsync(
                It.IsAny<CreateMonitorPollResultsInput>(),
                It.IsAny<IDbSession>(),
                It.IsAny<CancellationToken>()))
            .Callback<CreateMonitorPollResultsInput, IDbSession, CancellationToken>((input, session, _) =>
            {
                _createdMonitorPollResults = input;
                _createdMonitorPollResultsSession = session;
            })
            .Returns(Task.CompletedTask);

        _monitorCommands
            .Setup(c => c.UpdateAfterPollAsync(
                It.IsAny<UpdateMonitorAfterPollInput>(),
                It.IsAny<IDbSession>(),
                It.IsAny<CancellationToken>()))
            .Callback<UpdateMonitorAfterPollInput, IDbSession, CancellationToken>((input, session, _) =>
            {
                _updatedMonitor = input;
                _updatedMonitorSession = session;
            })
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _unitOfWork
            .Setup(u => u.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        _unitOfWorkFactory
            .Setup(f => f.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_unitOfWork.Object);

        _service = new PollingService(
            Mock.Of<ILogger<PollingService>>(),
            Options.Create(new PollingWorkerOptions
            {
                BatchSize = 50,
                LoopIntervalSeconds = 10
            }),
            _httpMonitorClient.Object,
            _jsonPathReader.Object,
            _monitorQueries.Object,
            _monitorCommands.Object,
            _monitorPollResultsCommands.Object,
            _unitOfWorkFactory.Object);
    }

    private void SetupDueMonitors(params DAL.Queries.Monitors.MonitorPollingRecord[] monitors)
        => _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitors);

    private void SetupHttpResponse(DAL.Queries.Monitors.MonitorPollingRecord monitor, HttpMonitorResponse response)
        => _httpMonitorClient
            .Setup(c => c.SendAsync(monitor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

    private void SetupJsonExtraction(string json, string path, bool succeeds, string? extractedValue)
    {
        string? value = extractedValue;

        _jsonPathReader
            .Setup(r => r.TryReadValue(json, path, out value))
            .Returns(succeeds);
    }

    private void AssertSavedPollResult(string? value, bool isSuccess, int? statusCode, int responseTimeMs, string requestStatus)
    {
        _createdMonitorPollResults.Should().NotBeNull();
        _createdMonitorPollResults!.Value.Should().Be(value);
        _createdMonitorPollResults.IsSuccess.Should().Be(isSuccess);
        _createdMonitorPollResults.StatusCode.Should().Be(statusCode);
        _createdMonitorPollResults.ResponseTimeMs.Should().Be(responseTimeMs);
        _createdMonitorPollResults.RequestStatus.Should().Be(requestStatus);
    }

    private void AssertMonitorUpdateCommandReceived(string? currentValue)
    {
        _updatedMonitor.Should().NotBeNull();
        _updatedMonitor!.CurrentValue.Should().Be(currentValue);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenMonitorSucceeds_PersistsResultAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = """
                   {
                    "data":
                        {
                            "status":"healthy"
                        }
                    }
                   """,
            StatusCode = 200
        };

        SetupDueMonitors(_monitor);
        SetupHttpResponse(_monitor, response);
        SetupJsonExtraction(response.Body, _monitor.ResultPath, succeeds: true, extractedValue: "healthy");

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        AssertSavedPollResult("healthy", isSuccess: true, statusCode: 200, responseTimeMs: 123, RequestStatusNames.Success);

        AssertMonitorUpdateCommandReceived("healthy");

        _updatedMonitor!.NextExecutionAt.Should()
            .Be(_updatedMonitor.LastCheckedAt.AddSeconds(_monitor.PollingIntervalSeconds));
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        _createdMonitorPollResultsSession.Should().BeSameAs(_unitOfWork.Object);
        _updatedMonitorSession.Should().BeSameAs(_unitOfWork.Object);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenHttpResponseFailed_DoesNotExtractValueAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: false,
            ResponseTimeMs: 222,
            RequestStatus: RequestStatusNames.Failed)
        {
            Body = """
                   {
                    "data":
                        {"status":"unhealthy"}
                   }
                   """,
            StatusCode = 500
        };

        SetupDueMonitors(_monitor);
        SetupHttpResponse(_monitor, response);

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _jsonPathReader.Verify(
            r => r.TryReadValue(It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string?>.IsAny),
            Times.Never);

        AssertSavedPollResult(null, isSuccess: false, statusCode: 500, responseTimeMs: 222, RequestStatusNames.Failed);
        AssertMonitorUpdateCommandReceived(null);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenValueExtractionFails_PersistsExtractionErrorAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = """{"data":"not-object"}""",
            StatusCode = 200
        };

        SetupDueMonitors(_monitor);
        SetupHttpResponse(_monitor, response);
        SetupJsonExtraction(response.Body, _monitor.ResultPath, succeeds: false, extractedValue: null);

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        AssertSavedPollResult(null, isSuccess: false, statusCode: 200, responseTimeMs: 123, RequestStatusNames.ExtractionError);
        AssertMonitorUpdateCommandReceived(null);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenExpectedValueIsMissing_PersistsExtractionErrorAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = """{"status":"ok"}""",
            StatusCode = 200
        };

        SetupDueMonitors(_monitor);
        SetupHttpResponse(_monitor, response);
        SetupJsonExtraction(response.Body, _monitor.ResultPath, succeeds: true, extractedValue: null);

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        AssertSavedPollResult(null, isSuccess: false, statusCode: 200, responseTimeMs: 123, RequestStatusNames.ExtractionError);
        AssertMonitorUpdateCommandReceived(null);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenSuccessfulResponseBodyIsEmpty_PersistsExtractionErrorAndUpdatesMonitorAsync()
    {
        // Arrange
        HttpMonitorResponse response = new(
            IsSuccess: true,
            ResponseTimeMs: 123,
            RequestStatus: RequestStatusNames.Success)
        {
            Body = "",
            StatusCode = 200
        };

        SetupDueMonitors(_monitor);
        SetupHttpResponse(_monitor, response);

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _jsonPathReader.Verify(
            r => r.TryReadValue(It.IsAny<string>(), It.IsAny<string>(), out It.Ref<string?>.IsAny),
            Times.Never);

        AssertSavedPollResult(null, isSuccess: false, statusCode: 200, responseTimeMs: 123, RequestStatusNames.ExtractionError);

        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenMultipleMonitorsAreDue_ProcessesEachMonitorAsync()
    {
        // Arrange
        DAL.Queries.Monitors.MonitorPollingRecord second = _monitor with { Id = Guid.NewGuid() };
        HttpMonitorResponse response = new(
            IsSuccess: false,
            ResponseTimeMs: 222,
            RequestStatus: RequestStatusNames.Failed)
        {
            StatusCode = 500
        };

        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_monitor, second]);
        _httpMonitorClient
            .Setup(c => c.SendAsync(It.IsAny<DAL.Queries.Monitors.MonitorPollingRecord>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _httpMonitorClient.Verify(
            c => c.SendAsync(It.IsAny<DAL.Queries.Monitors.MonitorPollingRecord>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _unitOfWorkFactory.Verify(
            f => f.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
        _unitOfWork.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenNoMonitorsAreDue_DoesNotProcessAnythingAsync()
    {
        // Arrange
        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _service.ProcessDueMonitorsAsync();

        // Assert
        _httpMonitorClient.Verify(
            c => c.SendAsync(It.IsAny<DAL.Queries.Monitors.MonitorPollingRecord>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _monitorPollResultsCommands.Verify(
            c => c.CreateAsync(
                It.IsAny<CreateMonitorPollResultsInput>(),
                It.IsAny<IDbSession>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ProcessDueMonitorsAsync_WhenCancellationIsRequested_ThrowsOperationCanceledExceptionAsync()
    {
        // Arrange
        _monitorQueries
            .Setup(q => q.GetDueEnabledAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([_monitor]);

        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act
        Func<Task> act = () => _service.ProcessDueMonitorsAsync(cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
