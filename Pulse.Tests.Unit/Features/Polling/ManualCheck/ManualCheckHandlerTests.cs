using FluentAssertions;
using FluentResults;
using Microsoft.Extensions.Logging;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Polling.ManualCheck;
using Pulse.BL.Features.Polling.ManualCheck.Queue;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Polling.ManualCheck;

public class ManualCheckHandlerTests
{
    private readonly Mock<IMonitorQueries> _monitorQueries = new();
    private readonly Mock<IManualCheckQueue> _queue = new();
    private readonly Mock<ICurrentUserService> _currentUserService = new();
    private readonly ILogger<ManualCheckHandler> _logger = Mock.Of<ILogger<ManualCheckHandler>>();

    private ManualCheckHandler CreateHandler()
        => new(_monitorQueries.Object, _queue.Object, _currentUserService.Object, _logger);

    [Fact]
    public async Task HandleAsync_WhenMonitorDoesNotExist_ReturnsNotFoundAndDoesNotEnqueue()
    {
        // Arrange
        ManualCheckHandler handler = CreateHandler();
        Guid monitorId = Guid.NewGuid();
        Guid organizationId = Guid.NewGuid();
        _currentUserService.SetupGet(service => service.OrganizationId).Returns(organizationId);

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorPollingRecord?)null);

        // Act
        Result result = await handler.HandleAsync(
            new ManualCheckCommand(monitorId, organizationId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<NotFoundError>();

        _queue.Verify(q => q.TryEnqueue(It.IsAny<ManualCheckCommand>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenMonitorExistsAndQueueHasCapacity_EnqueuesMonitorIdAndReturnsSuccess()
    {
        // Arrange
        ManualCheckHandler handler = CreateHandler();
        Guid organizationId = Guid.NewGuid();
        _currentUserService.SetupGet(service => service.OrganizationId).Returns(organizationId);

        MonitorPollingRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            10,
            "Enabled",
            Guid.NewGuid());

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _queue
            .Setup(q => q.TryEnqueue(new ManualCheckCommand(monitor.Id, organizationId)))
            .Returns(true);

        // Act
        Result result = await handler.HandleAsync(
            new ManualCheckCommand(monitor.Id, organizationId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _queue.Verify(q => q.TryEnqueue(new ManualCheckCommand(monitor.Id, organizationId)), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenQueueIsFull_ReturnsTooManyRequestsError()
    {
        // Arrange
        ManualCheckHandler handler = CreateHandler();
        Guid organizationId = Guid.NewGuid();
        _currentUserService.SetupGet(service => service.OrganizationId).Returns(organizationId);

        MonitorPollingRecord monitor = new(
            Guid.NewGuid(),
            "https://example.com/health",
            "GET",
            "status",
            60,
            10,
            "Enabled",
            Guid.NewGuid());

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _queue
            .Setup(q => q.TryEnqueue(new ManualCheckCommand(monitor.Id, organizationId)))
            .Returns(false);

        // Act
        Result result = await handler.HandleAsync(
            new ManualCheckCommand(monitor.Id, organizationId),
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<TooManyRequestsError>();
    }
}
