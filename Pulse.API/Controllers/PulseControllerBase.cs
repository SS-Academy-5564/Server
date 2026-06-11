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
            return StatusCode(500, BuildProblemDetails(500, "Unknown error", "INTERNAL_ERROR"));

        return error switch
        {
            NotFoundError e => NotFound(BuildProblemDetails(404, e.Message, e.Code)),
            ValidationError e => BadRequest(BuildProblemDetails(400, e.Message, e.Code)),
            UnauthorizedError e => Unauthorized(BuildProblemDetails(401, e.Message, e.Code)),
            ForbiddenError e => StatusCode(403, BuildProblemDetails(403, e.Message, e.Code)),
            ConflictError e => Conflict(BuildProblemDetails(409, e.Message, e.Code)),
            InternalError e => StatusCode(500, BuildProblemDetails(500, e.Message, e.Code)),
            _ => StatusCode(500, BuildProblemDetails(500, "Unexpected error", "INTERNAL_ERROR"))
        };
    }

    private static ProblemDetails BuildProblemDetails(int status, string detail, string code)
    {
        var pd = new ProblemDetails { Status = status, Detail = detail };
        pd.Extensions["code"] = code;
        return pd;
    }
}
