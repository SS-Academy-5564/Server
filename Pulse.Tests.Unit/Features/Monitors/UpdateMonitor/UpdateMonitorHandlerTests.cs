using System.Data;
using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Monitors;
using Pulse.BL.Features.Monitors.UpdateMonitor;
using Pulse.DAL.Commands.Monitors;
using Pulse.DAL.Common.Repository;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.UpdateMonitor;

public class UpdateMonitorHandlerTests
{
    private readonly Mock<IUnitOfWorkFactory> _uowFactoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<IMonitorCommands> _commandsMock = new();
    private readonly Mock<IMonitorQueries> _queriesMock = new();

    private readonly UpdateMonitorHandler _sut;

    private static readonly Guid MonitorId = Guid.NewGuid();

    public UpdateMonitorHandlerTests()
    {
        _uowFactoryMock
            .Setup(x => x.CreateAsync(It.IsAny<IsolationLevel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_uowMock.Object);

        _commandsMock
            .Setup(x => x.UpdateAsync(It.IsAny<UpdateMonitorInput>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(MonitorId);

        _sut = new UpdateMonitorHandler(_uowFactoryMock.Object, _commandsMock.Object, _queriesMock.Object);
    }

    private static UpdateMonitorCommand ValidCommand(Guid? id = null)
        => new(id ?? MonitorId, "EUR/USD Rate", "https://api.example.com/data", "GET", "data.usd.rate", BL.Features.Monitors.MonitorStatus.Enabled, 300, 10);

    private static MonitorRecord ExistingRecord(Guid? id = null)
        => new(
            id ?? MonitorId,
            "Old Name",
            "https://old.example.com",
            "GET",
            "old.path",
            null,
            Pulse.DAL.Queries.Monitors.MonitorStatus.Enabled,
            120,
            10,
            DateTimeOffset.UtcNow,
            DateTime.UtcNow);

    [Fact]
    public async Task HandleAsync_WhenMonitorNotFound_ReturnsNotFoundError()
    {
        _queriesMock
            .Setup(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorRecord?)null);

        Result<MonitorListResult> result = await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is NotFoundError);
    }

    [Fact]
    public async Task HandleAsync_WhenMonitorExists_ReturnsUpdatedMonitor()
    {
        _queriesMock
            .Setup(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingRecord());

        Result<MonitorListResult> result = await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(MonitorId);
        result.Value.Name.Should().Be("EUR/USD Rate");
        result.Value.Url.Should().Be("https://api.example.com/data");
        result.Value.Status.Should().Be(BL.Features.Monitors.MonitorStatus.Enabled);
        result.Value.Interval.Should().Be(300);
        result.Value.CurrentValue.Should().BeNull();
        result.Value.LastCheckedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_WhenMonitorExists_PassesInputToCommands()
    {
        _queriesMock
            .Setup(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingRecord());

        await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        _commandsMock.Verify(x => x.UpdateAsync(
            It.Is<UpdateMonitorInput>(i =>
                i.Id == MonitorId &&
                i.Name == "EUR/USD Rate" &&
                i.Url == "https://api.example.com/data" &&
                i.HttpMethod == "GET" &&
                i.ResultPath == "data.usd.rate" &&
                i.Status == BL.Features.Monitors.MonitorStatus.Enabled.ToString() &&
                i.PollingIntervalSeconds == 300 &&
                i.PollingTimeoutSeconds == 10),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMonitorExists_CommitsUnitOfWork()
    {
        _queriesMock
            .Setup(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingRecord());

        await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        _uowMock.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenMonitorNotFound_DoesNotCallUpdateCommand()
    {
        _queriesMock
            .Setup(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorRecord?)null);

        await _sut.HandleAsync(ValidCommand(), CancellationToken.None);

        _commandsMock.Verify(x => x.UpdateAsync(It.IsAny<UpdateMonitorInput>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
