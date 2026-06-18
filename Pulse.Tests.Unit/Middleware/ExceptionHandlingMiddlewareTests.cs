using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Pulse.API.Middleware;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;

namespace Pulse.Tests.Unit.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenValidationExceptionThrown_ReturnsValidationEnvelope()
    {
        RequestDelegate next = _ => throw new ValidationException(new[]
        {
            new ValidationFailure("Email", "Email is invalid")
        });

        ExceptionHandlingMiddleware middleware = new(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/json");

        ApiResponse response = await ReadBodyAsync(context);
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainSingle();
        response.Errors[0].Code.Should().Be(AppError.Codes.Validation);
        response.Errors[0].Field.Should().Be("Email");
        response.Errors[0].Message.Should().Be("Email is invalid");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessExceptionThrown_ReturnsUnauthorizedEnvelope()
    {
        RequestDelegate next = _ => throw new UnauthorizedAccessException();

        ExceptionHandlingMiddleware middleware = new(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().Be("application/json");

        ApiResponse response = await ReadBodyAsync(context);
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainSingle();
        response.Errors[0].Code.Should().Be(AppError.Codes.Unauthorized);
        response.Errors[0].Message.Should().Be("Unauthorized");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnexpectedExceptionThrown_ReturnsInternalEnvelope()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("boom");

        ExceptionHandlingMiddleware middleware = new(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/json");

        ApiResponse response = await ReadBodyAsync(context);
        response.Success.Should().BeFalse();
        response.Errors.Should().ContainSingle();
        response.Errors[0].Code.Should().Be(AppError.Codes.Internal);
        response.Errors[0].Message.Should().Be("An unexpected error occurred");
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseHasStarted_RethrowsExceptionAsync()
    {
        RequestDelegate next = context => throw new InvalidOperationException("started");

        ExceptionHandlingMiddleware middleware = new(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext context = CreateStartedContext();

        Func<Task> act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("started");
    }

    private static DefaultHttpContext CreateContext()
    {
        DefaultHttpContext context = new();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static DefaultHttpContext CreateStartedContext()
    {
        FeatureCollection features = new();
        features.Set<IHttpResponseFeature>(new StartedHttpResponseFeature());

        return new DefaultHttpContext(features);
    }

    private static async Task<ApiResponse> ReadBodyAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using StreamReader reader = new(context.Response.Body);
        string text = await reader.ReadToEndAsync();
        ApiResponse? response = JsonSerializer.Deserialize<ApiResponse>(text, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        response.Should().NotBeNull();
        return response!;
    }

    private sealed class StartedHttpResponseFeature : IHttpResponseFeature
    {
        public int StatusCode { get; set; } = StatusCodes.Status200OK;
        public string? ReasonPhrase { get; set; }
        public IHeaderDictionary Headers { get; set; } = new HeaderDictionary();
        public Stream Body { get; set; } = Stream.Null;
        public bool HasStarted => true;
        public void OnStarting(Func<object, Task> callback, object state) { }
        public void OnCompleted(Func<object, Task> callback, object state) { }
    }
}
