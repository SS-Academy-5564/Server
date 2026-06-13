using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Pulse.BL.Common.Errors;
using System.Text.Json;

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
                throw;

            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var (status, problem) = ex switch
        {
            ValidationException ve => MapValidation(ve),
            UnauthorizedAccessException => MapUnauthorized(),
            _ => MapInternal()
        };

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";

        var payload = JsonSerializer.Serialize((object)problem, problem.GetType(), JsonOptions);
        await context.Response.WriteAsync(payload);
    }

    private static (int, ProblemDetails) MapValidation(ValidationException ve)
    {
        var errors = ve.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var pd = new ValidationProblemDetails(errors)
        {
            Status = 400,
            Title = "Validation Error",
            Detail = "One or more validation errors occurred"
        };

        pd.Extensions["code"] = AppError.Codes.Validation;

        return (400, pd);
    }

    private static (int, ProblemDetails) MapUnauthorized() => (401, new ProblemDetails
    {
        Status = 401,
        Title = "Unauthorized",
        Detail = "Unauthorized",
        Extensions = { ["code"] = AppError.Codes.Unauthorized }
    });

    private static (int, ProblemDetails) MapInternal() => (500, new ProblemDetails
    {
        Status = 500,
        Title = "Internal Server Error",
        Detail = "An unexpected error occurred",
        Extensions = { ["code"] = AppError.Codes.Internal }
    });
}
