using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Pagination;
using Pulse.BL.Features.Monitors;
using Pulse.DAL.Common.Pagination;
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
        IReadOnlyList<MonitorListRecord> records = new List<MonitorListRecord>
        {
            new(Guid.NewGuid(), "Billing API", "https://api.com", "99%", DateTimeOffset.UtcNow, DAL.Queries.Monitors.MonitorStatus.Enabled, 60)
        }.AsReadOnly();

        _queriesMock.Setup(q => q.GetAllAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>(records, 21));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(new GetMonitorsQuery(null, null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Name.Should().Be("Billing API");
        result.Value.Items[0].Status.Should().Be(BL.Features.Monitors.MonitorStatus.Enabled);
        result.Value.Items[0].Interval.Should().Be(60);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(21);
        result.Value.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_WhenFilteredByStatus_PassesStatusToQueries()
    {
        IReadOnlyList<MonitorListRecord> records = new List<MonitorListRecord>().AsReadOnly();

        _queriesMock.Setup(q => q.GetAllAsync(DAL.Queries.Monitors.MonitorStatus.Disabled, 2, 25, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>(records, 0));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(
            new GetMonitorsQuery(BL.Features.Monitors.MonitorStatus.Disabled, 2, 25));

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();

        _queriesMock.Verify(
            q => q.GetAllAsync(DAL.Queries.Monitors.MonitorStatus.Disabled, 2, 25, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoMonitors_ReturnsEmptyList()
    {
        _queriesMock.Setup(q => q.GetAllAsync(null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>([], 0));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(new GetMonitorsQuery(null, null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }
}
