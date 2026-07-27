using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Monitors.UpdateMonitor;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;
using Pulse.BL.Features.Monitors.UpdateMonitor;

namespace Pulse.Tests.Unit.Features.Monitors.UpdateMonitor;

public class UpdateMonitorControllerTests
{
    private readonly Mock<IAsyncHandler<UpdateMonitorCommand, Result<MonitorListResult>>> _handlerMock = new();
    private readonly UpdateMonitorController _sut;

    public UpdateMonitorControllerTests()
    {
        _sut = new UpdateMonitorController(_handlerMock.Object);
    }

    private static readonly Guid MonitorId = Guid.NewGuid();

    private static UpdateMonitorRequest ValidRequest()
        => new("EUR/USD Rate", "https://api.example.com/data", "GET", "data.usd.rate", MonitorStatus.Enabled, 300, 10);

    [Fact]
    public async Task UpdateMonitor_WhenSuccess_Returns200WithUpdatedMonitor()
    {
        MonitorListResult updated = new(
            MonitorId, "EUR/USD Rate", "https://api.example.com/data", null, null, MonitorStatus.Enabled, 300);

        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMonitorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(updated));

        IActionResult result = await _sut.UpdateMonitorAsync(MonitorId, ValidRequest(), CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<MonitorListResult> response =
            ok.Value.Should().BeOfType<ApiResponse<MonitorListResult>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task UpdateMonitor_MapsRouteIdAndRequestFieldsToCommand()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMonitorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new MonitorListResult(
                MonitorId, "EUR/USD Rate", "https://api.example.com/data", null, null, MonitorStatus.Enabled, 300)));

        await _sut.UpdateMonitorAsync(MonitorId, ValidRequest(), CancellationToken.None);

        _handlerMock.Verify(h => h.HandleAsync(
            It.Is<UpdateMonitorCommand>(c =>
                c.Id == MonitorId &&
                c.Name == "EUR/USD Rate" &&
                c.Url == "https://api.example.com/data" &&
                c.HttpMethod == "GET" &&
                c.ResultPath == "data.usd.rate" &&
                c.Status == MonitorStatus.Enabled &&
                c.PollingIntervalSeconds == 300 &&
                c.PollingTimeoutSeconds == 10),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMonitor_WhenNotFound_Returns404()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMonitorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new NotFoundError("Monitor with this Id does not exist.")));

        IActionResult result = await _sut.UpdateMonitorAsync(MonitorId, ValidRequest(), CancellationToken.None);

        ObjectResult objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task UpdateMonitor_WhenValidationError_Returns400()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMonitorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new ValidationError("Invalid monitor.")));

        IActionResult result = await _sut.UpdateMonitorAsync(MonitorId, ValidRequest(), CancellationToken.None);

        ObjectResult objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }
}
