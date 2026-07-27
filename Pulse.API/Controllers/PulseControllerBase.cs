using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.API.Attributes;
using Pulse.API.Responses;
using Pulse.BL.Common.Pagination;

namespace Pulse.API.Controllers;

[AutoValidate]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status500InternalServerError)]
public abstract class PulseControllerBase : ControllerBase
{
    protected IActionResult ToActionResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse<T> { Success = true, Data = result.Value });
        }

        return MapErrorToResponse(result);
    }

    protected IActionResult ToActionResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(new ApiResponse { Success = true });
        }

        return MapErrorToResponse(result);
    }

    protected IActionResult ToPagedActionResult<T>(Result<PagedResult<T>> result)
    {
        if (result.IsSuccess)
        {
            PagedResult<T> page = result.Value;

            return Ok(new ApiResponse<IReadOnlyList<T>>
            {
                Success = true,
                Data = page.Items,
                Pagination = new ApiPagination
                {
                    Page = page.PageNumber,
                    PageSize = page.PageSize,
                    TotalCount = page.TotalCount
                }
            });
        }

        return MapErrorToResponse(result);
    }

    protected IActionResult MapErrorToResponse(ResultBase result)
    {
        (int statusCode, object? body) = ResultMapper.Map(result);
        return StatusCode(statusCode, body);
    }
}
