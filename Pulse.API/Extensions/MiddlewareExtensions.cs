using Pulse.API.Middleware;

namespace Pulse.API.Extensions;

public static class MiddlewareExtensions
{
    extension(IApplicationBuilder app)
    {
        public IApplicationBuilder UseResponseLogging()
        {
            return app.UseMiddleware<ResponseLoggingMiddleware>();
        }
    }
}
