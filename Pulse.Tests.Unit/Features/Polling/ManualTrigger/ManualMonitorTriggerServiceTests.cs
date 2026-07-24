using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Polling.ManualTrigger;
using Pulse.BL.Features.Polling.ManualTrigger.Queue;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Polling.ManualTrigger;

public class ManualMonitorTriggerServiceTests
{
    private readonly Mock<IMonitorQueries> _monitorQueries = new();
    private readonly Mock<IManualCheckQueue> _queue = new();
    private readonly ILogger<ManualMonitorTriggerService> _logger = Mock.Of<ILogger<ManualMonitorTriggerService>>();

    private ManualMonitorTriggerService CreateService()
        => new(_monitorQueries.Object, _queue.Object, _logger);

    [Fact]
    public async Task TriggerAsync_WhenMonitorDoesNotExist_ReturnsNotFoundAndDoesNotEnqueue()
    {
        // Arrange
        ManualMonitorTriggerService service = CreateService();
        Guid monitorId = Guid.NewGuid();

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorPollingRecord?)null);

        // Act
        Result result = await service.TriggerAsync(monitorId, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<NotFoundError>();

        _queue.Verify(q => q.TryEnqueue(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task TriggerAsync_WhenMonitorExistsAndQueueHasCapacity_EnqueuesMonitorIdAndReturnsSuccess()
    {
        // Arrange
        ManualMonitorTriggerService service = CreateService();

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

        _queue
            .Setup(q => q.TryEnqueue(monitor.Id))
            .Returns(true);

        // Act
        Result result = await service.TriggerAsync(monitor.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _queue.Verify(q => q.TryEnqueue(monitor.Id), Times.Once);
    }

    [Fact]
    public async Task TriggerAsync_WhenQueueIsFull_ReturnsTooManyRequestsError()
    {
        // Arrange
        ManualMonitorTriggerService service = CreateService();

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

        _queue
            .Setup(q => q.TryEnqueue(monitor.Id))
            .Returns(false);

        // Act
        Result result = await service.TriggerAsync(monitor.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<TooManyRequestsError>();
    }
}
