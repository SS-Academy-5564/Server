using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Features.Monitors;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.GetMonitors;

public class GetMonitorsQueryHandlerTests
{
    private readonly Mock<IMonitorQueries> _queriesMock;
    private readonly GetMonitorsQueryHandler _sut;

    public GetMonitorsQueryHandlerTests()
    {
        _queriesMock = new();
        _sut = new GetMonitorsQueryHandler(_queriesMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenRecordsExist_ReturnsMappedResults()
    {
        IReadOnlyList<MonitorRecord> records = new List<MonitorRecord>
        {
            new(Guid.NewGuid(), "Billing API", "https://api.com", "99%", DateTimeOffset.UtcNow, DAL.Queries.Monitors.MonitorStatus.Enabled, 60)
        }.AsReadOnly();

        _queriesMock.Setup(q => q.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        Result<IReadOnlyList<MonitorResult>> result = await _sut.HandleAsync(new GetMonitorsQuery(null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Name.Should().Be("Billing API");
        result.Value[0].Status.Should().Be(BL.Features.Monitors.MonitorStatus.Enabled);
        result.Value[0].Interval.Should().Be(60);
    }

    [Fact]
    public async Task HandleAsync_WhenFilteredByStatus_PassesStatusToQueries()
    {
        IReadOnlyList<MonitorRecord> records = new List<MonitorRecord>().AsReadOnly();

        _queriesMock.Setup(q => q.GetAllAsync(DAL.Queries.Monitors.MonitorStatus.Disabled, It.IsAny<CancellationToken>()))
            .ReturnsAsync(records);

        Result<IReadOnlyList<MonitorResult>> result = await _sut.HandleAsync(new GetMonitorsQuery(BL.Features.Monitors.MonitorStatus.Disabled));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();

        _queriesMock.Verify(q => q.GetAllAsync(DAL.Queries.Monitors.MonitorStatus.Disabled, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoMonitors_ReturnsEmptyList()
    {
        _queriesMock.Setup(q => q.GetAllAsync(null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<MonitorRecord>().AsReadOnly());

        Result<IReadOnlyList<MonitorResult>> result = await _sut.HandleAsync(new GetMonitorsQuery(null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
