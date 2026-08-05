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
    public async Task HandleAsync_WhenOrganizationIsMissing_ReturnsUnauthorizedAndDoesNotEnqueue()
    {
        // Arrange
        ManualCheckHandler handler = CreateHandler();
        _currentUserService.SetupGet(service => service.OrganizationId).Returns((Guid?)null);

        // Act
        Result result = await handler.HandleAsync(
            new ManualCheckCommand(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle().Which.Should().BeOfType<UnauthorizedError>();
        _monitorQueries.VerifyNoOtherCalls();
        _queue.VerifyNoOtherCalls();
    }

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
        Result result = await handler.HandleAsync(new ManualCheckCommand(monitorId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<NotFoundError>();

        _queue.Verify(q => q.TryEnqueue(It.IsAny<ManualCheckJob>()), Times.Never);
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
            "Enabled");

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _queue
            .Setup(q => q.TryEnqueue(new ManualCheckJob(monitor.Id, organizationId)))
            .Returns(true);

        // Act
        Result result = await handler.HandleAsync(new ManualCheckCommand(monitor.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _queue.Verify(q => q.TryEnqueue(new ManualCheckJob(monitor.Id, organizationId)), Times.Once);
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
            "Enabled");

        _monitorQueries
            .Setup(q => q.GetByIdForPollingAsync(monitor.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(monitor);

        _queue
            .Setup(q => q.TryEnqueue(new ManualCheckJob(monitor.Id, organizationId)))
            .Returns(false);

        // Act
        Result result = await handler.HandleAsync(new ManualCheckCommand(monitor.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle();
        result.Errors[0].Should().BeOfType<TooManyRequestsError>();
    }
}
