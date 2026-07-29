using FluentAssertions;
using FluentResults;
using Moq;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Pagination;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Monitors;
using Pulse.DAL.Common.Pagination;
using Pulse.DAL.Queries.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.GetMonitors;

public class GetMonitorsQueryHandlerTests
{
    private readonly Mock<IMonitorQueries> _queriesMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly GetMonitorsQueryHandler _sut;
    private static readonly Guid DefaultOrgId = Guid.Parse("B1000000-0000-0000-0000-000000000001");

    public GetMonitorsQueryHandlerTests()
    {
        _queriesMock = new();
        _currentUserServiceMock = new();
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns(DefaultOrgId);
        _sut = new GetMonitorsQueryHandler(_queriesMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenRecordsExist_ReturnsMappedResults()
    {
        IReadOnlyList<MonitorListRecord> records = new List<MonitorListRecord>
        {
            new(Guid.NewGuid(), "Billing API", "https://api.com", "99%", DateTimeOffset.UtcNow, DAL.Queries.Monitors.MonitorStatus.Enabled, 60, DefaultOrgId)
        }.AsReadOnly();

        _queriesMock.Setup(q => q.GetAllAsync(DefaultOrgId, null, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>(records, 21));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(new GetMonitorsQuery(DefaultOrgId, null, null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Name.Should().Be("Billing API");
        result.Value.Items[0].Status.Should().Be(BL.Features.Monitors.MonitorStatus.Enabled);
        result.Value.Items[0].Interval.Should().Be(60);
        result.Value.Items[0].OrganizationId.Should().Be(DefaultOrgId);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
        result.Value.TotalCount.Should().Be(21);
        result.Value.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_WhenFilteredByStatus_PassesStatusToQueries()
    {
        IReadOnlyList<MonitorListRecord> records = new List<MonitorListRecord>().AsReadOnly();

        _queriesMock.Setup(q => q.GetAllAsync(DefaultOrgId, DAL.Queries.Monitors.MonitorStatus.Disabled, 2, 25, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>(records, 0));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(
            new GetMonitorsQuery(DefaultOrgId, BL.Features.Monitors.MonitorStatus.Disabled, 2, 25));

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();

        _queriesMock.Verify(
            q => q.GetAllAsync(DefaultOrgId, DAL.Queries.Monitors.MonitorStatus.Disabled, 2, 25, null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilteredBySearch_PassesExactSearchStringToQueries()
    {
        const string searchString = "Billing API";
        GetMonitorsQuery query = new(DefaultOrgId, null, null, null) { SearchString = searchString };

        _queriesMock
            .Setup(q => q.GetAllAsync(DefaultOrgId, null, 1, 10, searchString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>([], 0));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        _queriesMock.Verify(
            q => q.GetAllAsync(DefaultOrgId, null, 1, 10, searchString, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenFilteredByStatusAndSearch_PassesBothFiltersToQueries()
    {
        const string searchString = "Payments";
        GetMonitorsQuery query = new(DefaultOrgId, BL.Features.Monitors.MonitorStatus.Disabled, 2, 25)
        {
            SearchString = searchString
        };

        _queriesMock
            .Setup(q => q.GetAllAsync(
                DefaultOrgId,
                DAL.Queries.Monitors.MonitorStatus.Disabled,
                2,
                25,
                searchString,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>([], 0));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        _queriesMock.Verify(
            q => q.GetAllAsync(
                DefaultOrgId,
                DAL.Queries.Monitors.MonitorStatus.Disabled,
                2,
                25,
                searchString,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenSearchIsCleared_PassesEmptySearchStringToQueries()
    {
        GetMonitorsQuery query = new(DefaultOrgId, null, null, null) { SearchString = string.Empty };

        _queriesMock
            .Setup(q => q.GetAllAsync(DefaultOrgId, null, 1, 10, string.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>([], 0));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(query);

        result.IsSuccess.Should().BeTrue();
        _queriesMock.Verify(
            q => q.GetAllAsync(DefaultOrgId, null, 1, 10, string.Empty, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoMonitors_ReturnsEmptyList()
    {
        _queriesMock.Setup(q => q.GetAllAsync(DefaultOrgId, null, 1, 10, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedRecords<MonitorListRecord>([], 0));

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(new GetMonitorsQuery(DefaultOrgId, null, null, null));

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_NoOrganizationId_ReturnsUnauthorizedError()
    {
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns((Guid?)null);

        Result<PagedResult<MonitorListResult>> result = await _sut.HandleAsync(new GetMonitorsQuery(DefaultOrgId, null, null, null));

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle(e => e is UnauthorizedError);
    }
}
