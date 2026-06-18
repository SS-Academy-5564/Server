using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;

namespace Pulse.Tests.Unit.Controllers;

public class PulseControllerBaseTests
{
    [Fact]
    public void ToActionResult_WhenValidationAndForbiddenErrorsExist_ReturnsForbidden()
    {
        Result result = Result.Fail(new List<IError>
        {
            new ValidationError("Validation failed"),
            new ForbiddenError("Access denied"),
            new InternalError("Unexpected")
        });

        TestController controller = new();

        IActionResult actionResult = controller.InvokeToActionResult(result);

        ObjectResult objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(403);

        ApiResponse response = objectResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainSingle();
        response.Errors[0].Message.Should().Be("Access denied");
        response.Errors[0].Code.Should().Be(AppError.Codes.Forbidden);
    }

    [Fact]
    public void ToActionResult_WhenUnauthorizedAndValidationErrorsExist_ReturnsUnauthorized()
    {
        Result result = Result.Fail(new List<IError>
        {
            new ValidationError("Validation failed"),
            new UnauthorizedError("Unauthorized")
        });

        TestController controller = new();

        IActionResult actionResult = controller.InvokeToActionResult(result);

        ObjectResult objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(401);

        ApiResponse response = objectResult.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainSingle();
        response.Errors[0].Message.Should().Be("Unauthorized");
        response.Errors[0].Code.Should().Be(AppError.Codes.Unauthorized);
    }

    [Fact]
    public void ToActionResult_WhenValidationErrorExists_ReturnsBadRequest()
    {
        Result result = Result.Fail(new ValidationError(
            "Validation failed",
            new Dictionary<string, string[]>
            {
                ["Email"] = ["Email is required"]
            }));

        TestController controller = new();

        IActionResult actionResult = controller.InvokeToActionResult(result);

        ObjectResult badRequest = actionResult.Should().BeOfType<ObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(400);

        ApiResponse response = badRequest.Value.Should().BeOfType<ApiResponse>().Subject;
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainSingle();
        response.Errors[0].Code.Should().Be(AppError.Codes.Validation);
        response.Errors[0].Field.Should().Be("Email");
        response.Errors[0].Message.Should().Be("Email is required");
    }

    [Fact]
    public void ToActionResult_WhenResultIsSuccess_ReturnsSuccessResponseEnvelope()
    {
        TestController controller = new();

        IActionResult actionResult = controller.InvokeToActionResult(Result.Ok());

        OkObjectResult okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ApiResponse response = okResult.Value.Should().BeOfType<ApiResponse>().Subject;

        response.Success.Should().BeTrue();
        response.Errors.Should().BeEmpty();
    }

    private sealed class TestController : PulseControllerBase
    {
        public IActionResult InvokeToActionResult(Result result) => ToActionResult(result);
    }
}
