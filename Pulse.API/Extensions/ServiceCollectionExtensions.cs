using System.Threading.RateLimiting;
using Pulse.API.Constants;

namespace Pulse.API.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLoginRateLimiter(IConfiguration configuration)
        {
            var rateLimiterSection = configuration.GetSection("RateLimit.Login");
            var permitLimit = rateLimiterSection.GetValue<int>("PermitLimit");
            var windowMinutes = rateLimiterSection.GetValue<int>("Window");

            services.AddRateLimiter(options =>
            {
                options.AddPolicy(RateLimitPolicies.Login, context =>
                {

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = permitLimit,
                            Window = TimeSpan.FromMinutes(windowMinutes),
                            QueueLimit = 0,
                        }
                    );
                });

                options.OnRejected = async (onRejectedContext, cancellationToken) =>
                {
                    var httpContext = onRejectedContext.HttpContext;

                    string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown IP";
                    var logger = httpContext.RequestServices.GetRequiredService<ILogger<RateLimiter>>();
                    var path = httpContext.Request.Path;

                    logger.LogWarning("Rate limit exceeded for IP: {IP} on Path: {Path}", ipAddress, path);

                    httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    if (onRejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        httpContext.Response.Headers.RetryAfter = retryAfter.TotalSeconds.ToString();
                    }
                    httpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds)).ToString();

                    await httpContext.Response.WriteAsJsonAsync(new
                    {
                        Error = "Too many requests",
                        RetryAfter = retryAfter.TotalSeconds
                    }, cancellationToken);
                };
            });
            return services;
        }
    }
}
