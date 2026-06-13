using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Filters;
using Pulse.BL.Common.Errors;

namespace Pulse.API.Controllers;

[AutoValidate]
public abstract class PulseControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return MapErrorToResponse(result.Errors);
    }

    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return MapErrorToResponse(result.Errors);
    }

    private IActionResult MapErrorToResponse(IReadOnlyList<IError> errors)
    {
        var error = errors.FirstOrDefault();

        if (error is null)
            return StatusCode(500, BuildProblemDetails(500, "Internal Server Error", "Unknown error", AppError.Codes.Internal));

        return error switch
        {
            ValidationError e => BadRequest(BuildValidationProblemDetails(e)),
            NotFoundError e => NotFound(BuildProblemDetails(404, "Not Found", e.Message, e.Code)),
            UnauthorizedError e => Unauthorized(BuildProblemDetails(401, "Unauthorized", e.Message, e.Code)),
            ForbiddenError e => StatusCode(403, BuildProblemDetails(403, "Forbidden", e.Message, e.Code)),
            ConflictError e => Conflict(BuildProblemDetails(409, "Conflict", e.Message, e.Code)),
            InternalError e => StatusCode(500, BuildProblemDetails(500, "Internal Server Error", e.Message, e.Code)),
            _ => StatusCode(500, BuildProblemDetails(500, "Internal Server Error", "Unexpected error", AppError.Codes.Internal))
        };
    }

    private static ValidationProblemDetails BuildValidationProblemDetails(ValidationError error)
    {
        var errors = error.FieldErrors is { Count: > 0 }
            ? new Dictionary<string, string[]>(error.FieldErrors)
            : new Dictionary<string, string[]> { [string.Empty] = [error.Message] };

        var pd = new ValidationProblemDetails(errors)
        {
            Status = 400,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred"
        };

        pd.Extensions["code"] = error.Code;

        return pd;
    }

    private static ProblemDetails BuildProblemDetails(int status, string title, string detail, string code)
    {
        var pd = new ProblemDetails { Status = status, Title = title, Detail = detail };
        pd.Extensions["code"] = code;
        return pd;
    }
}
