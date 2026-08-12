using System.Data;
using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Monitors;
using Pulse.BL.Features.Monitors.UpdateMonitorStatus;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.Tests.Unit.Features.Monitors.UpdateMonitorStatus;

public class UpdateMonitorStatusHandlerTests
{
    private readonly Mock<IUnitOfWorkFactory> _uowFactoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMonitorCommands> _monitorCommandsMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly UpdateMonitorStatusHandler _sut;

    public UpdateMonitorStatusHandlerTests()
    {
        _uowFactoryMock
            .Setup(x => x.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uowMock.Object);

        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns(Guid.Parse("B1000000-0000-0000-0000-000000000001"));

        _monitorCommandsMock
            .Setup(x => x.UpdateStatusAsync(
                It.IsAny<UpdateMonitorStatusInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _sut = new UpdateMonitorStatusHandler(
            _uowFactoryMock.Object,
            _monitorCommandsMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsEnabled_CommitsAndReturnsOk()
    {
        Result result = await _sut.HandleAsync(new UpdateMonitorStatusCommand(Guid.NewGuid(), MonitorStatus.Enabled), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _monitorCommandsMock.Verify(x => x.UpdateStatusAsync(
            It.IsAny<UpdateMonitorStatusInput>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsDisabled_CommitsAndReturnsOk()
    {
        Result result = await _sut.HandleAsync(new UpdateMonitorStatusCommand(Guid.NewGuid(), MonitorStatus.Disabled), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _monitorCommandsMock.Verify(x => x.UpdateStatusAsync(
            It.IsAny<UpdateMonitorStatusInput>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenStatusIsError_ReturnsValidationError()
    {
        Result result = await _sut.HandleAsync(new UpdateMonitorStatusCommand(Guid.NewGuid(), MonitorStatus.Error), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is ValidationError);
        _monitorCommandsMock.Verify(x => x.UpdateStatusAsync(
            It.IsAny<UpdateMonitorStatusInput>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNoRowsAffected_ReturnsNotFoundErrorAndDoesNotCommit()
    {
        _monitorCommandsMock
            .Setup(x => x.UpdateStatusAsync(
                It.IsAny<UpdateMonitorStatusInput>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        Result result = await _sut.HandleAsync(new UpdateMonitorStatusCommand(Guid.NewGuid(), MonitorStatus.Enabled), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is NotFoundError);
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_WhenNoOrganizationId_ReturnsUnauthorizedError()
    {
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns((Guid?)null);

        Result result = await _sut.HandleAsync(new UpdateMonitorStatusCommand(Guid.NewGuid(), MonitorStatus.Enabled), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UnauthorizedError);
        _monitorCommandsMock.Verify(x => x.UpdateStatusAsync(
            It.IsAny<UpdateMonitorStatusInput>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_PassesOrganizationIdAndStatusToCommands()
    {
        Guid monitorId = Guid.NewGuid();
        Guid organizationId = Guid.Parse("B1000000-0000-0000-0000-000000000001");

        await _sut.HandleAsync(new UpdateMonitorStatusCommand(monitorId, MonitorStatus.Disabled), CancellationToken.None);

        _monitorCommandsMock.Verify(x => x.UpdateStatusAsync(
            It.Is<UpdateMonitorStatusInput>(input =>
                input.MonitorId == monitorId &&
                input.OrganizationId == organizationId &&
                input.Status == MonitorStatus.Disabled.ToString()),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
