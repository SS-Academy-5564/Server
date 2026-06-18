using System.Text.Json;
using FluentValidation;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;

namespace Pulse.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);

            if (context.Response.HasStarted)
            {
                throw;
            }

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        (int status, ApiResponse body) = ex switch
        {
            ValidationException ve => MapValidation(ve),
            UnauthorizedAccessException => MapUnauthorized(),
            _ => MapInternal()
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        string payload = JsonSerializer.Serialize((object)body, body.GetType(), JsonOptions);
        await context.Response.WriteAsync(payload);
    }

    private static (int, ApiResponse) MapValidation(ValidationException ve)
    {
        ApiError[] errors = ve.Errors
            .Select(error => new ApiError
            {
                Code = AppError.Codes.Validation,
                Field = error.PropertyName,
                Message = error.ErrorMessage
            })
            .ToArray();

        return (400, new ApiResponse
        {
            Success = false,
            Errors = errors
        });
    }

    private static (int, ApiResponse) MapUnauthorized() => (401, new ApiResponse
    {
        Success = false,
        Errors =
        [
            new ApiError
            {
                Code = AppError.Codes.Unauthorized,
                Message = "Unauthorized"
            }
        ]
    });

    private static (int, ApiResponse) MapInternal() => (500, new ApiResponse
    {
        Success = false,
        Errors =
        [
            new ApiError
            {
                Code = AppError.Codes.Internal,
                Message = "An unexpected error occurred"
            }
        ]
    });
}
