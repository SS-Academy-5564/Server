using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Monitors.GetMonitors;
using Pulse.API.Responses;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.GetMonitors;

public class GetMonitorsControllerTests
{
    private readonly Mock<IAsyncHandler<GetMonitorsQuery, Result<IReadOnlyList<MonitorResult>>>> _handlerMock;
    private readonly GetMonitorsController _sut;

    public GetMonitorsControllerTests()
    {
        _handlerMock = new();
        _sut = new GetMonitorsController(_handlerMock.Object);
    }

    [Fact]
    public async Task GetMonitors_WhenSuccess_Returns200WithDtoShape()
    {
        IReadOnlyList<MonitorResult> monitors = new List<MonitorResult>
        {
            new(Guid.NewGuid(), "Billing API", "https://api.com", "99%", DateTimeOffset.UtcNow, MonitorStatus.Enabled, 60)
        }.AsReadOnly();

        _handlerMock.Setup(h => h.HandleAsync(It.IsAny<GetMonitorsQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(monitors));

        IActionResult result = await _sut.GetMonitorsAsync(new GetMonitorsRequest(null), CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        ApiResponse<IReadOnlyList<MonitorResult>> response = ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<MonitorResult>>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(monitors);
    }

    [Fact]
    public async Task GetMonitors_WithStatusFilter_PassesStatusToHandler()
    {
        IReadOnlyList<MonitorResult> monitors = new List<MonitorResult>().AsReadOnly();

        _handlerMock
            .Setup(h => h.HandleAsync(It.Is<GetMonitorsQuery>(q => q.Status == MonitorStatus.Disabled), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(monitors));

        IActionResult result = await _sut.GetMonitorsAsync(new GetMonitorsRequest(MonitorStatus.Disabled), CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        _handlerMock.Verify(h => h.HandleAsync(
            It.Is<GetMonitorsQuery>(q => q.Status == MonitorStatus.Disabled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMonitors_WhenNoFilter_ReturnsAllMonitorsIncludingError()
    {
        IReadOnlyList<MonitorResult> monitors = new List<MonitorResult>
        {
            new(Guid.NewGuid(), "Billing API", "https://api.com", "99%", DateTimeOffset.UtcNow, MonitorStatus.Enabled, 60),
            new(Guid.NewGuid(), "Broken Service", "https://broken.com", null, DateTimeOffset.UtcNow, MonitorStatus.Error, 120),
        }.AsReadOnly();

        _handlerMock
            .Setup(h => h.HandleAsync(It.Is<GetMonitorsQuery>(q => q.Status == null), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(monitors));

        IActionResult result = await _sut.GetMonitorsAsync(new GetMonitorsRequest(null), CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<IReadOnlyList<MonitorResult>> response = ok.Value.Should().BeOfType<ApiResponse<IReadOnlyList<MonitorResult>>>().Subject;
        response.Data.Should().Contain(m => m.Status == MonitorStatus.Error);
    }
}
