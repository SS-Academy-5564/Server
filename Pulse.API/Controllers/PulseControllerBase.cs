using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Filters;
using Pulse.API.Responses;

namespace Pulse.API.Controllers;

[AutoValidate]
public abstract class PulseControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return MapErrorToResponse(result);
    }

    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return MapErrorToResponse(result);
    }

    public IActionResult MapErrorToResponse(ResultBase result)
    {
        var (statusCode, body) = ResultMapper.Map(result);
        return StatusCode(statusCode, body);
    }
}
