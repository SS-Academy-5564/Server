using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Auth.Registration;
using Pulse.API.Responses;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.Registration;

namespace Pulse.Tests.Unit.Features.Auth.Registration;

public class RegistrationControllerTests
{
    private readonly Mock<IAsyncHandler<RegistrationCommand, Result<RegistrationResult>>> _handler = new();
    private readonly RegistrationController _controller;

    public RegistrationControllerTests()
    {
        _controller = new RegistrationController(_handler.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task RegisterAsync_UkrainianAcceptLanguage_PassesUkrainianToHandler()
    {
        RegistrationRequest request = ValidRequest();
        _handler
            .Setup(handler => handler.HandleAsync(
                It.Is<RegistrationCommand>(command => command.Language == "uk"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new RegistrationResult(60)));

        IActionResult result = await _controller.RegisterAsync(
            request,
            CancellationToken.None,
            "en;q=0.5,uk-UA;q=0.9");

        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<RegistrationResult> response = okResult.Value
            .Should().BeOfType<ApiResponse<RegistrationResult>>().Subject;
        response.Data.Should().Be(new RegistrationResult(60));
        _handler.Verify(handler => handler.HandleAsync(
            It.Is<RegistrationCommand>(command =>
                command.Email == request.Email &&
                command.FirstName == request.FirstName &&
                command.LastName == request.LastName &&
                command.Password == request.Password &&
                command.Language == "uk"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("fr-FR,de;q=0.8")]
    public async Task RegisterAsync_MissingOrUnsupportedAcceptLanguage_PassesEnglishToHandler(string? acceptLanguage)
    {
        RegistrationRequest request = ValidRequest();
        _handler
            .Setup(handler => handler.HandleAsync(
                It.Is<RegistrationCommand>(command => command.Language == "en"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new RegistrationResult(60)));

        IActionResult result = await _controller.RegisterAsync(
            request,
            CancellationToken.None,
            acceptLanguage);

        result.Should().BeOfType<OkObjectResult>();
        _handler.Verify(handler => handler.HandleAsync(
            It.Is<RegistrationCommand>(command => command.Language == "en"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    private static RegistrationRequest ValidRequest()
        => new("john.doe@example.com", "John", "Doe", "SecurePass1");
}
