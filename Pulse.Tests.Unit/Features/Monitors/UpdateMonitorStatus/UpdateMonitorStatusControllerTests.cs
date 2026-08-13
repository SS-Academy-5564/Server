using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Monitors.UpdateMonitorStatus;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Monitors;
using Pulse.BL.Features.Monitors.UpdateMonitorStatus;

namespace Pulse.Tests.Unit.Features.Monitors.UpdateMonitorStatus;

public class UpdateMonitorStatusControllerTests
{
    private readonly Mock<IAsyncHandler<UpdateMonitorStatusCommand, Result>> _handlerMock = new();
    private readonly UpdateMonitorStatusController _sut;

    public UpdateMonitorStatusControllerTests()
    {
        _sut = new UpdateMonitorStatusController(_handlerMock.Object);
    }

    private static UpdateMonitorStatusRequest ValidRequest() => new(MonitorStatus.Disabled);

    [Fact]
    public async Task UpdateMonitorStatus_WhenSuccess_Returns200()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMonitorStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        IActionResult result = await _sut.UpdateMonitorStatusAsync(Guid.NewGuid(), ValidRequest(), CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse response = ok.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateMonitorStatus_MapsRequestFieldsToCommand()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMonitorStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Guid monitorId = Guid.NewGuid();

        await _sut.UpdateMonitorStatusAsync(monitorId, ValidRequest(), CancellationToken.None);

        _handlerMock.Verify(h => h.HandleAsync(
            It.Is<UpdateMonitorStatusCommand>(c =>
                c.MonitorId == monitorId &&
                c.Status == MonitorStatus.Disabled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateMonitorStatus_WhenValidationError_Returns400()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<UpdateMonitorStatusCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new ValidationError("Invalid status.")));

        IActionResult result = await _sut.UpdateMonitorStatusAsync(Guid.NewGuid(), ValidRequest(), CancellationToken.None);

        ObjectResult objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }
}
