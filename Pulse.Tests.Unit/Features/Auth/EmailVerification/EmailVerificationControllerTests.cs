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
    private readonly Mock<IAsyncHandler<VerifyEmailCommand, Result>> _handler = new();

    [Fact]
    public async Task VerifyAsync_ValidToken_ReturnsOk()
    {
        const string token = "valid-token";
        _handler
            .Setup(h => h.HandleAsync(
                It.Is<VerifyEmailCommand>(command => command.Token == token),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        EmailVerificationController controller = new(_handler.Object);

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
        _handler
            .Setup(h => h.HandleAsync(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Fail(error));
        EmailVerificationController controller = new(_handler.Object);

        IActionResult result = await controller.VerifyAsync(
            new VerifyEmailRequest("token"),
            CancellationToken.None);

        ObjectResult objectResult = result.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(expectedStatus);
        ApiResponse response = objectResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Errors.Should().ContainSingle().Which.Code.Should().Be(expectedCode);
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
}
