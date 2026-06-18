using FluentResults;
using Pulse.BL.Common.Errors;

namespace Pulse.API.Responses;

internal static class ResultMapper
{
    internal static (int StatusCode, object Body) Map(ResultBase result)
    {
        if (result.Errors.Count == 0)
            return BuildErrorResponse(
                500,
                AppError.Codes.Internal,
                "An unexpected error occurred"
            );

        if (result.HasError<ForbiddenError>())
        {
            var error = result.Errors.OfType<ForbiddenError>().First();
            return BuildErrorResponse(403, error.Code, error.Message);
        }

        if (result.HasError<UnauthorizedError>())
        {
            var error = result.Errors.OfType<UnauthorizedError>().First();
            return BuildErrorResponse(401, error.Code, error.Message);
        }

        if (result.HasError<ConflictError>())
        {
            var error = result.Errors.OfType<ConflictError>().First();
            return BuildErrorResponse(409, error.Code, error.Message);
        }

        if (result.HasError<NotFoundError>())
        {
            var error = result.Errors.OfType<NotFoundError>().First();
            return BuildErrorResponse(404, error.Code, error.Message);
        }

        if (result.HasError<ValidationError>())
        {
            var error = result.Errors.OfType<ValidationError>().First();
            return BuildValidationError(error);
        }

        if (result.HasError<InternalError>())
        {
            var error = result.Errors.OfType<InternalError>().First();
            return BuildErrorResponse(500, error.Code, error.Message);
        }

        return BuildErrorResponse(500, AppError.Codes.Internal, "An unexpected error occurred");
    }

    private static (int StatusCode, object Body) BuildValidationError(ValidationError error)
    {
        if (error.FieldErrors.Count == 0)
        {
            return BuildErrorResponse(400, error.Code, error.Message);
        }

        var fieldErrors = error.FieldErrors
            .SelectMany(field => field.Value.Select(message => new ApiError
            {
                Code = error.Code,
                Field = field.Key,
                Message = message
            }))
            .ToArray();

        return (400, new ApiResponse
        {
            Success = false,
            Errors = fieldErrors
        });
    }

    private static (int StatusCode, object Body) BuildErrorResponse(int statusCode, string code, string message)
    {
        return (statusCode, new ApiResponse
        {
            Success = false,
            Errors =
            [
                new ApiError
                {
                    Code = code,
                    Message = message
                }
            ]
        });
    }
}
