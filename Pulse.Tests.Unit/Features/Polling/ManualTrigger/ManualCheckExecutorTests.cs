using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.ManualTrigger;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Polling.ManualTrigger;

public class ManualCheckExecutorTests
{
    private readonly Mock<IMonitorQueries> _monitorQueries = new();
    private readonly Mock<IPollingService> _pollingService = new();
    private readonly ILogger<ManualCheckExecutor> _logger = Mock.Of<ILogger<ManualCheckExecutor>>();

    private ManualCheckExecutor CreateExecutor()
        => new(_monitorQueries.Object, _pollingService.Object, _logger);

    [Fact]
    public async Task ExecuteAsync_WhenMonitorIsStillEligible_ReRequeriesAndProcessesIt()
    {
        // Arrange
        ManualCheckExecutor executor = CreateExecutor();

        MonitorPollingRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            10,
            "Enabled");

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _pollingService
            .Setup(p => p.ProcessMonitorAsync(monitor, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        await executor.ExecuteAsync(monitor.Id, CancellationToken.None);

        // Assert
        _monitorQueries.Verify(
            q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()),
            Times.Once);
        _pollingService.Verify(
            p => p.ProcessMonitorAsync(monitor, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMonitorIsNoLongerEligible_SkipsProcessingWithoutThrowing()
    {
        // Arrange
        ManualCheckExecutor executor = CreateExecutor();
        Guid monitorId = Guid.NewGuid();

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorPollingRecord?)null);

        // Act
        Func<Task> act = () => executor.ExecuteAsync(monitorId, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        _pollingService.Verify(
            p => p.ProcessMonitorAsync(It.IsAny<MonitorPollingRecord>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
