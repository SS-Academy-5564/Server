using System.Text.Json;
using FluentAssertions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Controllers;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Pagination;

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

    /// <summary>
    /// Verifies that paged responses expose the page number under the client-facing JSON contract.
    /// </summary>
    [Fact]
    public void ToPagedActionResult_WhenResultIsSuccess_SerializesPageNumber()
    {
        PagedResult<string> page = new(["member"], 2, 10, 21);
        TestController controller = new();

        IActionResult actionResult = controller.InvokeToPagedActionResult(Result.Ok(page));

        OkObjectResult okResult = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        string json = JsonSerializer.Serialize(okResult.Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using JsonDocument document = JsonDocument.Parse(json);
        JsonElement pagination = document.RootElement.GetProperty("pagination");

        pagination.GetProperty("pageNumber").GetInt32().Should().Be(2);
        pagination.TryGetProperty("page", out _).Should().BeFalse();
    }

    private sealed class TestController : PulseControllerBase
    {
        public IActionResult InvokeToActionResult(Result result) => ToActionResult(result);

        /// <summary>
        /// Invokes the protected ToPagedActionResult method on the base controller.
        /// </summary>
        /// <typeparam name="T">The type of items in the paged result.</typeparam>
        /// <param name="result">The result from the business layer.</param>
        /// <returns>An IActionResult representing the HTTP response.</returns>
        public IActionResult InvokeToPagedActionResult<T>(Result<PagedResult<T>> result) => ToPagedActionResult(result);
    }
}
