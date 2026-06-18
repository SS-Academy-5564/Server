using System.Text.Json;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Pulse.API.Middleware;
using Pulse.BL.Common.Errors;

namespace Pulse.Tests.Unit.Middleware;

public class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenValidationExceptionThrown_ReturnsValidationProblemDetailsAsync()
    {
        RequestDelegate next = _ => throw new ValidationException(new[]
        {
            new ValidationFailure("Email", "Email is invalid")
        });

        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        context.Response.ContentType.Should().Be("application/problem+json");

        JsonElement json = await ReadBodyAsJsonAsync(context);
        json.GetProperty("title").GetString().Should().Be("Validation Error");
        json.GetProperty("status").GetInt32().Should().Be(400);
        json.GetProperty("detail").GetString().Should().Be("One or more validation errors occurred");
        json.GetProperty("code").GetString().Should().Be(AppError.Codes.Validation);
        JsonElement errorsProperty = GetPropertyIgnoreCase(json, "errors");
        errorsProperty.TryGetProperty("Email", out JsonElement emailErrors).Should().BeTrue();
        emailErrors[0].GetString().Should().Be("Email is invalid");
    }

    [Fact]
    public async Task InvokeAsync_WhenUnauthorizedAccessExceptionThrown_ReturnsUnauthorizedProblemDetailsAsync()
    {
        RequestDelegate next = _ => throw new UnauthorizedAccessException();

        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        context.Response.ContentType.Should().Be("application/problem+json");

        JsonElement json = await ReadBodyAsJsonAsync(context);
        json.GetProperty("title").GetString().Should().Be("Unauthorized");
        json.GetProperty("status").GetInt32().Should().Be(401);
        json.GetProperty("detail").GetString().Should().Be("Unauthorized");
        json.GetProperty("code").GetString().Should().Be(AppError.Codes.Unauthorized);
    }

    [Fact]
    public async Task InvokeAsync_WhenUnexpectedExceptionThrown_ReturnsInternalProblemDetailsAsync()
    {
        RequestDelegate next = _ => throw new InvalidOperationException("boom");

        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        context.Response.ContentType.Should().Be("application/problem+json");

        JsonElement json = await ReadBodyAsJsonAsync(context);
        json.GetProperty("title").GetString().Should().Be("Internal Server Error");
        json.GetProperty("status").GetInt32().Should().Be(500);
        json.GetProperty("detail").GetString().Should().Be("An unexpected error occurred");
        json.GetProperty("code").GetString().Should().Be(AppError.Codes.Internal);
    }

    [Fact]
    public async Task InvokeAsync_WhenResponseHasStarted_RethrowsExceptionAsync()
    {
        RequestDelegate next = context => throw new InvalidOperationException("started");

        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);
        DefaultHttpContext context = CreateStartedContext();

        Func<Task> act = () => middleware.InvokeAsync(context);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("started");
    }

    private static DefaultHttpContext CreateContext()
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        return context;
    }

    private static DefaultHttpContext CreateStartedContext()
    {
        var features = new FeatureCollection();
        features.Set<IHttpResponseFeature>(new StartedHttpResponseFeature());

        return new DefaultHttpContext(features);
    }

    private static async Task<JsonElement> ReadBodyAsJsonAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        string text = await reader.ReadToEndAsync();
        using var document = JsonDocument.Parse(text);
        return document.RootElement.Clone();
    }

    private static JsonElement GetPropertyIgnoreCase(JsonElement element, string propertyName)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                return property.Value;
            }
        }

        throw new KeyNotFoundException($"Property '{propertyName}' was not found.");
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
