using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Auth.PasswordReset;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Features.Auth.PasswordReset.RequestCode;
using Pulse.BL.Features.Auth.PasswordReset.ResetPassword;
using Pulse.BL.Features.Auth.PasswordReset.VerifyCode;

namespace Pulse.Tests.Unit.Features.Auth.PasswordReset;

public class PasswordResetControllerTests
{
    private readonly Mock<IRequestPasswordResetHandler> _requestHandlerMock;
    private readonly Mock<IVerifyPasswordResetCodeHandler> _verifyHandlerMock;
    private readonly Mock<IResetPasswordHandler> _resetHandlerMock;
    private readonly PasswordResetController _sut;

    public PasswordResetControllerTests()
    {
        _requestHandlerMock = new Mock<IRequestPasswordResetHandler>();
        _verifyHandlerMock = new Mock<IVerifyPasswordResetCodeHandler>();
        _resetHandlerMock = new Mock<IResetPasswordHandler>();

        _sut = new PasswordResetController(
            _requestHandlerMock.Object,
            _verifyHandlerMock.Object,
            _resetHandlerMock.Object);
    }

    [Fact]
    public async Task RequestCodeAsync_ReturnsOk()
    {
        // Arrange
        RequestPasswordResetRequest request = new("test@example.com");

        _requestHandlerMock
            .Setup(x => x.RequestAsync(It.Is<RequestPasswordResetCommand>(c => c.Email == request.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        IActionResult result = await _sut.RequestCodeAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyCodeAsync_OnSuccess_ReturnsOkWithToken()
    {
        // Arrange
        VerifyPasswordResetCodeRequest request = new("test@example.com", "123456");
        VerifyCodeResult verifyResult = new("my_reset_token");

        _verifyHandlerMock
            .Setup(x => x.VerifyAsync(It.Is<VerifyPasswordResetCodeCommand>(c => c.Email == request.Email && c.Code == request.Code), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(verifyResult));

        // Act
        IActionResult result = await _sut.VerifyCodeAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse<VerifyCodeResult>>()
            .Which.Data.Should().Be(verifyResult);
    }

    [Fact]
    public async Task VerifyCodeAsync_OnFailure_ReturnsBadRequest()
    {
        // Arrange
        VerifyPasswordResetCodeRequest request = new("test@example.com", "invalid!");

        _verifyHandlerMock
            .Setup(x => x.VerifyAsync(It.IsAny<VerifyPasswordResetCodeCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new ValidationError("Invalid code.")));

        // Act
        IActionResult result = await _sut.VerifyCodeAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(400);
        ((ObjectResult)result).Value.Should().BeOfType<ApiResponse>()
            .Which.Success.Should().BeFalse();
    }

    [Fact]
    public async Task ResetPasswordAsync_OnSuccess_ReturnsOk()
    {
        // Arrange
        ResetPasswordRequest request = new("valid_token", "NewPassword123!", "NewPassword123!");

        _resetHandlerMock
            .Setup(x => x.ResetAsync(It.Is<ResetPasswordCommand>(c => c.ResetToken == request.ResetToken && c.NewPassword == request.NewPassword), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        // Act
        IActionResult result = await _sut.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ResetPasswordAsync_OnFailure_ReturnsUnauthorized()
    {
        // Arrange
        ResetPasswordRequest request = new("invalid_token", "NewPassword123!", "NewPassword123!");

        _resetHandlerMock
            .Setup(x => x.ResetAsync(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(new UnauthorizedError("Invalid token.")));

        // Act
        IActionResult result = await _sut.ResetPasswordAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(401);
        ((ObjectResult)result).Value.Should().BeOfType<ApiResponse>()
            .Which.Success.Should().BeFalse();
    }
}
