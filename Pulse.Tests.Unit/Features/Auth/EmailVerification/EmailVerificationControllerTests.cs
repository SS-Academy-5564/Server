using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Pulse.API.Features.Auth.EmailVerification;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Handlers;
using Pulse.BL.Features.Auth.EmailVerification;

namespace Pulse.Tests.Unit.Features.Auth.EmailVerification;

public class EmailVerificationControllerTests
{
    private readonly Mock<IAsyncHandler<VerifyEmailCommand, Result>> _verifyHandler = new();
    private readonly Mock<IAsyncHandler<ResendEmailVerificationCommand, Result<ResendEmailVerificationResult>>>
        _resendHandler = new();

    [Fact]
    public async Task VerifyAsync_ValidToken_ReturnsOk()
    {
        const string token = "valid-token";
        _verifyHandler
            .Setup(h => h.HandleAsync(
                It.Is<VerifyEmailCommand>(command => command.Token == token),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        EmailVerificationController controller = CreateController();

        IActionResult result = await controller.VerifyAsync(
            new VerifyEmailRequest(token),
            CancellationToken.None);

        result.Should().BeOfType<OkObjectResult>()
            .Which.Value.Should().BeOfType<ApiResponse>()
            .Which.Success.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(TokenFailureCases))]
    public async Task VerifyAsync_TokenFailure_ReturnsDistinctStatusAndCode(
        IError error,
        int expectedStatus,
        string expectedCode)
    {
        _verifyHandler
            .Setup(h => h.HandleAsync(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(error));
        EmailVerificationController controller = CreateController();

        IActionResult result = await controller.VerifyAsync(
            new VerifyEmailRequest("token"),
            CancellationToken.None);

        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(expectedStatus);
        ApiResponse response = objectResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Errors.Should().ContainSingle().Which.Code.Should().Be(expectedCode);
    }

    [Fact]
    public async Task ResendAsync_ExpiredToken_ReturnsCooldownGuidance()
    {
        const string token = "expired-token";
        _resendHandler
            .Setup(handler => handler.HandleAsync(
                It.Is<ResendEmailVerificationCommand>(command =>
                    command.Token == token &&
                    command.Language == "uk"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(new ResendEmailVerificationResult(60)));
        EmailVerificationController controller = CreateController();

        IActionResult result = await controller.ResendAsync(
            new ResendEmailVerificationRequest(token),
            CancellationToken.None,
            "uk-UA,uk;q=0.9,en;q=0.8");

        OkObjectResult okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse<ResendEmailVerificationResult> response = okResult.Value
            .Should().BeOfType<ApiResponse<ResendEmailVerificationResult>>().Subject;
        response.Success.Should().BeTrue();
        response.Data!.ResendCooldownSeconds.Should().Be(60);
    }

    public static TheoryData<IError, int, string> TokenFailureCases => new()
    {
        {
            new ValidationError(
                "The email verification token is invalid.",
                code: AppError.Codes.EmailVerificationTokenInvalid),
            400,
            AppError.Codes.EmailVerificationTokenInvalid
        },
        {
            new ValidationError(
                "The email verification token has expired.",
                code: AppError.Codes.EmailVerificationTokenExpired),
            400,
            AppError.Codes.EmailVerificationTokenExpired
        },
        {
            new ConflictError(
                "The email verification token has already been used.",
                AppError.Codes.EmailVerificationTokenAlreadyUsed),
            409,
            AppError.Codes.EmailVerificationTokenAlreadyUsed
        }
    };

    private EmailVerificationController CreateController()
        => new(_verifyHandler.Object, _resendHandler.Object);
}
