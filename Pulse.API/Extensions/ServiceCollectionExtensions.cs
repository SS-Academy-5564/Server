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

            services.AddRateLimiter(options =>
            {
                options.AddPolicy(RateLimitPolicies.Login, context =>
                {
                    var partitionKey = context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";

                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: partitionKey,
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = rateLimiterSection.GetValue<int>("PermitLimit"),
                            Window = TimeSpan.FromMinutes(rateLimiterSection.GetValue<int>("Window")),
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
                        httpContext.Response.Headers.RetryAfter = retryAfter.TotalMinutes.ToString();
                    }

                    await httpContext.Response.WriteAsync("Too many requests", cancellationToken);
                };
            });
            return services;
        }
    }
}
