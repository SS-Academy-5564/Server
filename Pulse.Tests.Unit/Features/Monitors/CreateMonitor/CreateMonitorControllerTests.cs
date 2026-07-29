using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Monitors.CreateMonitor;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Common.Security;
using Pulse.BL.Features.Monitors;

namespace Pulse.Tests.Unit.Features.Monitors.CreateMonitor;

public class CreateMonitorControllerTests
{
    private readonly Mock<IAsyncHandler<CreateMonitorCommand, Result<MonitorListResult>>> _handlerMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
    private readonly CreateMonitorController _sut;

    public CreateMonitorControllerTests()
    {
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns(Guid.Parse("B1000000-0000-0000-0000-000000000001"));
        _sut = new CreateMonitorController(_handlerMock.Object, _currentUserServiceMock.Object);
    }

    private static CreateMonitorRequest ValidRequest()
        => new("EUR/USD Rate", "https://api.example.com/data", "GET", "data.usd.rate", 300, 10);

    [Fact]
    public async Task CreateMonitor_WhenSuccess_Returns200WithCreatedMonitor()
    {
        MonitorListResult created = new(
            Guid.NewGuid(), "EUR/USD Rate", "https://api.example.com/data", null, null, MonitorStatus.Enabled, 300, Guid.Parse("B1000000-0000-0000-0000-000000000001"));

        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<CreateMonitorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(created));

        IActionResult result = await _sut.CreateMonitorAsync(ValidRequest(), CancellationToken.None);

        OkObjectResult ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<MonitorListResult> response =
            ok.Value.Should().BeOfType<ApiResponse<MonitorListResult>>().Subject;
        response.Success.Should().BeTrue();
        response.Data.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task CreateMonitor_MapsRequestFieldsToCommand()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<CreateMonitorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new MonitorListResult(
                Guid.NewGuid(), "EUR/USD Rate", "https://api.example.com/data", null, null, MonitorStatus.Enabled, 300, Guid.Parse("B1000000-0000-0000-0000-000000000001"))));

        await _sut.CreateMonitorAsync(ValidRequest(), CancellationToken.None);

        _handlerMock.Verify(h => h.HandleAsync(
            It.Is<CreateMonitorCommand>(c =>
                c.Name == "EUR/USD Rate" &&
                c.Url == "https://api.example.com/data" &&
                c.HttpMethod == "GET" &&
                c.ResultPath == "data.usd.rate" &&
                c.PollingIntervalSeconds == 300 &&
                c.PollingTimeoutSeconds == 10 &&
                c.OrganizationId == Guid.Parse("B1000000-0000-0000-0000-000000000001")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateMonitor_WhenValidationError_Returns400()
    {
        _handlerMock
            .Setup(h => h.HandleAsync(It.IsAny<CreateMonitorCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new ValidationError("Invalid monitor.")));

        IActionResult result = await _sut.CreateMonitorAsync(ValidRequest(), CancellationToken.None);

        ObjectResult objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task CreateMonitor_NoOrganizationId_Returns401()
    {
        _currentUserServiceMock
            .Setup(x => x.OrganizationId)
            .Returns((Guid?)null);

        IActionResult result = await _sut.CreateMonitorAsync(ValidRequest(), CancellationToken.None);

        ObjectResult objectResult = result.Should().BeAssignableTo<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(401);
    }
}
