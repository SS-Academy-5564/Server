using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
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

        var controller = new TestController();

        var actionResult = controller.InvokeToActionResult(result);

        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(403);

        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Be("Access denied");
        problem.Extensions["code"].Should().Be(AppError.Codes.Forbidden);
    }

    [Fact]
    public void ToActionResult_WhenUnauthorizedAndValidationErrorsExist_ReturnsUnauthorized()
    {
        Result result = Result.Fail(new List<IError>
        {
            new ValidationError("Validation failed"),
            new UnauthorizedError("Unauthorized")
        });

        var controller = new TestController();

        var actionResult = controller.InvokeToActionResult(result);

        var objectResult = actionResult.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(401);

        var problem = objectResult.Value.Should().BeOfType<ProblemDetails>().Subject;
        problem.Detail.Should().Be("Unauthorized");
        problem.Extensions["code"].Should().Be(AppError.Codes.Unauthorized);
    }

    [Fact]
    public void ToActionResult_WhenValidationErrorExists_ReturnsBadRequest()
    {
        Result result = Result.Fail(new ValidationError("Validation failed"));

        var controller = new TestController();

        var actionResult = controller.InvokeToActionResult(result);

        var badRequest = actionResult.Should().BeOfType<ObjectResult>().Subject;
        var problem = badRequest.Value.Should().BeOfType<ValidationProblemDetails>().Subject;
        problem.Status.Should().Be(400);
        problem.Extensions["code"].Should().Be(AppError.Codes.Validation);
    }

    private sealed class TestController : PulseControllerBase
    {
        public IActionResult InvokeToActionResult(Result result) => ToActionResult(result);
    }
}
