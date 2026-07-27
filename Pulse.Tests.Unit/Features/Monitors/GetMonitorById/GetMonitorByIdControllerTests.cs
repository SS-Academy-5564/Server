using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Monitors.GetMonitorById;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;
using Pulse.BL.Features.Monitors.GetMonitorById;

namespace Pulse.Tests.Unit.Features.Monitors.GetMonitorById;

public class GetMonitorByIdControllerTests
{
    private readonly Mock<IAsyncHandler<GetMonitorByIdQuery, Result<MonitorResult>>> _handlerMock = new();
    private readonly GetMonitorByIdController _sut;

    private static readonly Guid MonitorId = Guid.NewGuid();

    public GetMonitorByIdControllerTests()
    {
        _sut = new GetMonitorByIdController(_handlerMock.Object);
    }

    private static MonitorResult SampleResult()
        => new(
            MonitorId,
            "Billing API",
            "https://api.example.com/billing",
            "GET",
            "data.value",
            "99%",
            MonitorStatus.Enabled,
            60,
            10,
            DateTimeOffset.UtcNow,
            DateTime.UtcNow);

    [Fact]
    public async Task GetMonitorById_WhenSuccess_Returns200WithMonitorResult()
    {
        MonitorResult monitor = SampleResult();

        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<GetMonitorByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(monitor));

        IActionResult result = await _sut.GetMonitorByIdAsync(MonitorId, CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(200);

        ApiResponse<MonitorResult> response = ok.Value.Should().BeOfType<ApiResponse<MonitorResult>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(monitor);
    }

    [Fact]
    public async Task GetMonitorById_PassesRouteIdToHandler()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<GetMonitorByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(SampleResult()));

        await _sut.GetMonitorByIdAsync(MonitorId, CancellationToken.None);

        _handlerMock.Verify(h => h.HandleAsync(
            It.Is<GetMonitorByIdQuery>(q => q.Id == MonitorId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetMonitorById_WhenNotFound_Returns404()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<GetMonitorByIdQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new NotFoundError("Monitor with this Id does not exist.")));

        IActionResult result = await _sut.GetMonitorByIdAsync(MonitorId, CancellationToken.None);

        ObjectResult objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(404);
    }
}
