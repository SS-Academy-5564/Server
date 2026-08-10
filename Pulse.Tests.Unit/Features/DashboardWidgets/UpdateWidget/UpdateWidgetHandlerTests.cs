using System.Data;
using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.DashboardWidgets.UpdateWidget;
using Pulse.DAL.Commands.DashboardWidgets;
using Pulse.DAL.Commands.DashboardWidgets.UpdateWidget;
using Pulse.DAL.Common.Repository;

namespace Pulse.Tests.Unit.Features.DashboardWidgets.UpdateWidget;

public class UpdateWidgetHandlerTests
{
    private readonly Mock<IUnitOfWorkFactory> _uowFactoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IWidgetCommands> _commandsMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    private readonly UpdateWidgetHandler _sut;

    public UpdateWidgetHandlerTests()
    {
        _uowFactoryMock
            .Setup(x => x.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uowMock.Object);

        _commandsMock
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateWidgetInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns(Guid.Parse("B1000000-0000-0000-0000-000000000001"));

        _sut = new UpdateWidgetHandler(
            _uowFactoryMock.Object,
            _commandsMock.Object,
            _currentUserServiceMock.Object);
    }

    private static UpdateWidgetCommand ValidCommand()
        => new(
            Guid.Parse("00000000-0000-0000-0000-000000000001"),
            "LineChart",
            "Response Time",
            "Last 24 Hours",
            "ResponseTime",
            "24h",
            "{}"
        );

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsSuccess()
    {
        Result result =
            await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_PassesInputToCommands()
    {
        await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        _commandsMock.Verify(x => x.UpdateAsync(
            It.Is<UpdateWidgetInput>(i =>
                i.Id == Guid.Parse("00000000-0000-0000-0000-000000000001") &&
                i.Type == "LineChart" &&
                i.Metric == "ResponseTime" &&
                i.TimeRange == "24h" &&
                i.Settings == "{}" &&
                i.OrganizationId == Guid.Parse("B1000000-0000-0000-0000-000000000001")),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CommitsUnitOfWork()
    {
        await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        _uowMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WidgetNotFound_ReturnsNotFoundError()
    {
        _commandsMock
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateWidgetInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Result result =
            await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is NotFoundError);

        _uowMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NoOrganizationId_ReturnsUnauthorizedError()
    {
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns((Guid?)null);

        Result result =
            await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UnauthorizedError);

        _commandsMock.Verify(
            x => x.UpdateAsync(It.IsAny<UpdateWidgetInput>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _uowMock.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
