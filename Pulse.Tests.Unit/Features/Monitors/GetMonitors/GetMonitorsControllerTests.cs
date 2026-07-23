using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Monitors.GetMonitors;
using Pulse.API.Responses;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Pagination;
using Pulse.BL.Features.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.GetMonitors;

public class GetMonitorsControllerTests
{
    private readonly Mock<IAsyncHandler<GetMonitorsQuery, Result<PagedResult<MonitorListResult>>>> _handlerMock;
    private readonly GetMonitorsController _sut;

    public GetMonitorsControllerTests()
    {
        _handlerMock = new();
        _sut = new GetMonitorsController(_handlerMock.Object);
    }

    [Fact]
    public async Task GetMonitors_WhenSuccess_Returns200WithDtoShape()
    {
        IReadOnlyList<MonitorListResult> monitors = new List<MonitorListResult>
        {
            new(Guid.NewGuid(), "Billing API", "https://api.com", "99%", DateTimeOffset.UtcNow, MonitorStatus.Enabled, 60)
        }.AsReadOnly();

        _handlerMock.Setup(h => h.HandleAsync(It.IsAny<GetMonitorsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new PagedResult<MonitorListResult>(monitors, 1, 10, 21)));

        IActionResult result = await _sut.GetMonitorsAsync(new GetMonitorsRequest(null, null, null), CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        ApiResponse<IReadOnlyList<MonitorListResult>> response = ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<MonitorListResult>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(monitors);
        response.Pagination.Should().NotBeNull();
        response.Pagination!.Page.Should().Be(1);
        response.Pagination.PageSize.Should().Be(10);
        response.Pagination.TotalCount.Should().Be(21);
        response.Pagination.TotalPages.Should().Be(3);
    }

    [Fact]
    public async Task GetMonitors_WithStatusFilter_PassesStatusToHandler()
    {
        IReadOnlyList<MonitorListResult> monitors = new List<MonitorListResult>().AsReadOnly();

        _handlerMock
            .Setup(h => h.HandleAsync(
                It.Is<GetMonitorsQuery>(q =>
                    q.Status == MonitorStatus.Disabled &&
                    q.PageNumber == 2 &&
                    q.PageSize == 25),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new PagedResult<MonitorListResult>(monitors, 2, 25, 0)));

        IActionResult result = await _sut.GetMonitorsAsync(
            new GetMonitorsRequest(MonitorStatus.Disabled, 2, 25),
            CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        _handlerMock.Verify(h => h.HandleAsync(
            It.Is<GetMonitorsQuery>(q =>
                q.Status == MonitorStatus.Disabled &&
                q.PageNumber == 2 &&
                q.PageSize == 25),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMonitors_WhenNoFilter_ReturnsAllMonitorsIncludingError()
    {
        IReadOnlyList<MonitorListResult> monitors = new List<MonitorListResult>
        {
            new(Guid.NewGuid(), "Billing API", "https://api.com", "99%", DateTimeOffset.UtcNow, MonitorStatus.Enabled, 60),
            new(Guid.NewGuid(), "Broken Service", "https://broken.com", null, DateTimeOffset.UtcNow, MonitorStatus.Error, 120),
        }.AsReadOnly();

        _handlerMock
            .Setup(h => h.HandleAsync(It.Is<GetMonitorsQuery>(q => q.Status == null), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new PagedResult<MonitorListResult>(monitors, 1, 10, 2)));

        IActionResult result = await _sut.GetMonitorsAsync(new GetMonitorsRequest(null, null, null), CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<IReadOnlyList<MonitorListResult>> response = ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<MonitorListResult>>>().Subject;
        response.Data.Should().Contain(m => m.Status == MonitorStatus.Error);
    }
}
