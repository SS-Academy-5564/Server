using FluentResults;
using Pulse.BL.Common.Errors;

namespace Pulse.API.Responses;

internal static class ResultMapper
{
    internal static (int StatusCode, object Body) Map(ResultBase result)
    {
        if (result.HasError<ForbiddenError>())
        {
            ForbiddenError error = result.Errors.OfType<ForbiddenError>().First();
            return BuildErrorResponse(403, error.Code, error.Message);
        }

        if (result.HasError<UnauthorizedError>())
        {
            UnauthorizedError error = result.Errors.OfType<UnauthorizedError>().First();
            return BuildErrorResponse(401, error.Code, error.Message);
        }

        if (result.HasError<ConflictError>())
        {
            ConflictError error = result.Errors.OfType<ConflictError>().First();
            return BuildErrorResponse(409, error.Code, error.Message);
        }

        if (result.HasError<TooManyRequestsError>())
        {
            TooManyRequestsError error = result.Errors.OfType<TooManyRequestsError>().First();
            return BuildErrorResponse(429, error.Code, error.Message);
        }

        if (result.HasError<NotFoundError>())
        {
            NotFoundError error = result.Errors.OfType<NotFoundError>().First();
            return BuildErrorResponse(404, error.Code, error.Message);
        }

        if (result.HasError<ValidationError>())
        {
            ValidationError error = result.Errors.OfType<ValidationError>().First();
            return BuildValidationError(error);
        }

        if (result.HasError<InternalError>())
        {
            InternalError error = result.Errors.OfType<InternalError>().First();
            return BuildErrorResponse(500, error.Code, error.Message);
        }

        throw new InvalidOperationException("Unhandled error type in ResultMapper");
    }

    private static (int StatusCode, object Body) BuildValidationError(ValidationError error)
    {
        if (error.FieldErrors.Count == 0)
        {
            return BuildErrorResponse(400, error.Code, error.Message);
        }

        ApiError[] fieldErrors = error.FieldErrors
            .Where(field => field.Value != null && field.Value.Any())
            .SelectMany(field => field.Value.Select(message => new ApiError
            {
                Code = error.Code,
                Field = field.Key,
                Message = message
            }))
            .ToArray();

        if (fieldErrors.Length == 0)
        {
            fieldErrors = new[]
            {
                new ApiError { Code = error.Code, Message = "An unexpected validation error occurred" }
            };
        }

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
