using System.Globalization;
using System.Threading.RateLimiting;
using Pulse.API.Constants;

namespace Pulse.API.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLoginRateLimiter(IConfiguration configuration)
        {
            IConfigurationSection rateLimiterSection = configuration.GetSection("RateLimit:Login");

            int maxAttempts = rateLimiterSection.GetValue<int>("MaxAttempts");
            int periodMinutes = rateLimiterSection.GetValue<int>("PeriodMinutes");

            if (maxAttempts <= 0)
            {
                throw new InvalidOperationException(
                    "RateLimit:Login:MaxAttempts is missing or invalid. It must be greater than zero.");
            }

            if (periodMinutes <= 0)
            {
                throw new InvalidOperationException(
                    "RateLimit:Login:PeriodMinutes is missing or invalid. It must be greater than zero.");
            }

            services.AddRateLimiter(options =>
            {
                options.AddPolicy(RateLimitPolicies.Login, context =>
                {
                    return RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: GetClientIdentifier(context),
                        factory: _ => new TokenBucketRateLimiterOptions()
                        {
                            TokenLimit = maxAttempts,
                            TokensPerPeriod = 1,
                            ReplenishmentPeriod = TimeSpan.FromSeconds(periodMinutes * 60 / maxAttempts),
                            QueueLimit = 0,
                            AutoReplenishment = true
                        }
                    );
                });

                options.OnRejected = async (onRejectedContext, cancellationToken) =>
                {
                    HttpContext httpContext = onRejectedContext.HttpContext;
                    httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    double retryAfterSeconds = 0;
                    if (onRejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
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
    private static string GetClientIdentifier(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
}
