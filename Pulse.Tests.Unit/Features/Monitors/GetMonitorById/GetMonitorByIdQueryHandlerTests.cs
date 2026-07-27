using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Monitors.GetMonitorById;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.GetMonitorById;

public class GetMonitorByIdQueryHandlerTests
{
    private readonly Mock<IMonitorQueries> _queriesMock = new();
    private readonly GetMonitorByIdQueryHandler _sut;

    private static readonly Guid MonitorId = Guid.NewGuid();

    public GetMonitorByIdQueryHandlerTests()
    {
        _sut = new GetMonitorByIdQueryHandler(_queriesMock.Object);
    }

    private static MonitorRecord ExistingRecord(Guid? id = null)
        => new(
            id ?? MonitorId,
            "Billing API",
            "https://api.example.com/billing",
            "GET",
            "data.value",
            "99%",
            DAL.Queries.Monitors.MonitorStatus.Enabled,
            60,
            10,
            DateTimeOffset.UtcNow,
            DateTime.UtcNow,
            DateTimeOffset.UtcNow.AddDays(-7),
            DateTimeOffset.UtcNow.AddHours(-1));

    [Fact]
    public async Task HandleAsync_WhenMonitorExists_ReturnsMappedResult()
    {
        MonitorRecord record = ExistingRecord();
        _queriesMock
            .Setup(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        Result<MonitorResult> result = await _sut.HandleAsync(new GetMonitorByIdQuery(MonitorId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(record.Id);
        result.Value.Name.Should().Be(record.Name);
        result.Value.Url.Should().Be(record.Url);
        result.Value.HttpMethod.Should().Be(record.HttpMethod);
        result.Value.ResultPath.Should().Be(record.ResultPath);
        result.Value.CurrentValue.Should().Be(record.CurrentValue);
        result.Value.Status.Should().Be(BL.Features.Monitors.MonitorStatus.Enabled);
        result.Value.PollingIntervalSeconds.Should().Be(record.PollingIntervalSeconds);
        result.Value.PollingTimeoutSeconds.Should().Be(record.PollingTimeoutSeconds);
        result.Value.LastCheckedAt.Should().Be(record.LastCheckedAt);
        result.Value.NextExecutionAt.Should().Be(record.NextExecutionAt);
        result.Value.CreatedAt.Should().Be(record.CreatedAt);
        result.Value.LastModifiedAt.Should().Be(record.LastModifiedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenMonitorNotFound_ReturnsNotFoundError()
    {
        _queriesMock
            .Setup(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MonitorRecord?)null);

        Result<MonitorResult> result = await _sut.HandleAsync(new GetMonitorByIdQuery(MonitorId), CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is NotFoundError);
    }

    [Fact]
    public async Task HandleAsync_PassesIdToQueries()
    {
        _queriesMock
            .Setup(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingRecord());

        await _sut.HandleAsync(new GetMonitorByIdQuery(MonitorId), CancellationToken.None);

        _queriesMock.Verify(q => q.GetByIdAsync(MonitorId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
