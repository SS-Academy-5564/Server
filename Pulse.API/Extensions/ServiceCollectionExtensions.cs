using System.Globalization;
using System.Threading.RateLimiting;
using Pulse.API.Constants;

namespace Pulse.API.Extensions;

public static class ServiceCollectionExtensions
{
    private const int DefaultMaxAttempts = 20;
    private const int DefaultPeriodMinutes = 15;
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLoginRateLimiter(IConfiguration configuration)
        {
            var rateLimiterSection = configuration.GetSection("RateLimit.Login");

            int maxAttempts = rateLimiterSection.GetValue<int?>("MaxAttempts") ?? DefaultMaxAttempts;
            int periodMinutes = rateLimiterSection.GetValue<int?>("PeriodMinutes") ?? DefaultPeriodMinutes;

            services.AddRateLimiter(options =>
            {
                options.AddPolicy(RateLimitPolicies.Login, context =>
                {
                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: GetClientIdentifier(context),
                        factory: _ => new TokenBucketRateLimiterOptions()
                        {
                            TokenLimit = maxAttempts,
                            TokensPerPeriod = maxAttempts,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(periodMinutes),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }
                    );
                });

                options.OnRejected = async (onRejectedContext, cancellationToken) =>
                {
                    var httpContext = onRejectedContext.HttpContext;
                    var ipAddress = GetClientIdentifier(httpContext);
                    var path = httpContext.Request.Path;
                    var logger = httpContext.RequestServices
                        .GetRequiredService<ILoggerFactory>()
                        .CreateLogger(nameof(RateLimitPolicies.Login));

                    logger.LogWarning("Rate limit exceeded for IP: {IP} on Path: {Path}", ipAddress, path);

                    httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    double retryAfterSeconds = 0;
                    if (onRejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        retryAfterSeconds = Math.Ceiling(retryAfter.TotalSeconds);
                        httpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
                    }

                    await httpContext.Response.WriteAsJsonAsync(new
                    {
                        Error = "Too many requests",
                        RetryAfter = retryAfterSeconds
                    }, cancellationToken);
                };
            });
            return services;
        }
    }
    private static string GetClientIdentifier(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
}
