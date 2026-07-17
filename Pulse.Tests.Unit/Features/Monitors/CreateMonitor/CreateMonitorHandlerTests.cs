using System.Data;
using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Features.Monitors;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;

namespace Pulse.Tests.Unit.Features.Monitors.CreateMonitor;

public class CreateMonitorHandlerTests
{
    private readonly Mock<IUnitOfWorkFactory> _uowFactoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMonitorCommands> _commandsMock = new();

    private readonly CreateMonitorHandler _sut;

    public CreateMonitorHandlerTests()
    {
        _uowFactoryMock
            .Setup(x => x.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uowMock.Object);

        _commandsMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateMonitorInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.NewGuid());

        _sut = new CreateMonitorHandler(_uowFactoryMock.Object, _commandsMock.Object);
    }

    private static CreateMonitorCommand ValidCommand()
        => new("EUR/USD Rate", "https://api.example.com/data", "GET", "data.usd.rate", 300, 10);

    [Fact]
    public async Task HandleAsync_ValidCommand_ReturnsCreatedMonitorInEnabledStatus()
    {
        Guid expectedId = Guid.NewGuid();
        _commandsMock
            .Setup(x => x.CreateAsync(It.IsAny<CreateMonitorInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedId);

        Result<MonitorListResult> result = await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(expectedId);
        result.Value.Name.Should().Be("EUR/USD Rate");
        result.Value.Url.Should().Be("https://api.example.com/data");
        result.Value.Status.Should().Be(MonitorStatus.Enabled);
        result.Value.Interval.Should().Be(300);
        result.Value.CurrentValue.Should().BeNull();
        result.Value.LastCheckedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_PassesInputToCommands()
    {
        await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        _commandsMock.Verify(x => x.CreateAsync(
            It.Is<CreateMonitorInput>(i =>
                i.Name == "EUR/USD Rate" &&
                i.Url == "https://api.example.com/data" &&
                i.HttpMethod == "GET" &&
                i.ResultPath == "data.usd.rate" &&
                i.PollingIntervalSeconds == 300 &&
                i.PollingTimeoutSeconds == 10),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CommitsUnitOfWork()
    {
        await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
