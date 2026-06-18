using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Pulse.BL.Common.Errors;

namespace Pulse.API.Responses;

internal static class ResultMapper
{
    internal static (int StatusCode, object Body) Map(ResultBase result)
    {
        if (result.Errors.Count == 0)
        {
            return (500, BuildProblemDetails(500, "Internal Server Error", "Unknown error", AppError.Codes.Internal));
        }

        if (result.HasError<ForbiddenError>())
        {
            ForbiddenError error = result.Errors.OfType<ForbiddenError>().First();
            return (403, BuildProblemDetails(403, "Forbidden", error.Message, error.Code));
        }

        if (result.HasError<UnauthorizedError>())
        {
            UnauthorizedError error = result.Errors.OfType<UnauthorizedError>().First();
            return (401, BuildProblemDetails(401, "Unauthorized", error.Message, error.Code));
        }

        if (result.HasError<ConflictError>())
        {
            ConflictError error = result.Errors.OfType<ConflictError>().First();
            return (409, BuildProblemDetails(409, "Conflict", error.Message, error.Code));
        }

        if (result.HasError<NotFoundError>())
        {
            NotFoundError error = result.Errors.OfType<NotFoundError>().First();
            return (404, BuildProblemDetails(404, "Not Found", error.Message, error.Code));
        }

        if (result.HasError<ValidationError>())
        {
            ValidationError error = result.Errors.OfType<ValidationError>().First();
            return (400, BuildValidationProblemDetails(error));
        }

        if (result.HasError<InternalError>())
        {
            InternalError error = result.Errors.OfType<InternalError>().First();
            return (500, BuildProblemDetails(500, "Internal Server Error", error.Message, error.Code));
        }

        return (500, BuildProblemDetails(500, "Internal Server Error", "Unexpected error", AppError.Codes.Internal));
    }

    private static ValidationProblemDetails BuildValidationProblemDetails(ValidationError error)
    {
        Dictionary<string, string[]> errors = error.FieldErrors.Count > 0
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
