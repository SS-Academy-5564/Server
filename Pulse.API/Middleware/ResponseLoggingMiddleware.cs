namespace Pulse.API.Middleware;

public class ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);

        if (context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
        {
            var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
            var path = context.Request.Path;

            logger.LogWarning("Rate limit exceeded for IP: {IP} on Path: {Path}", ipAddress, path);
        }
    }
}
