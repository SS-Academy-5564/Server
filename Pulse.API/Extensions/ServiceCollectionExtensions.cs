using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.OpenApi;
using Pulse.API.Constants;
using Pulse.API.Documentation;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;

namespace Pulse.API.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddLoginRateLimiter(IConfiguration configuration)
        {
            var rateLimiterSection = configuration.GetSection("RateLimit:Login");

            var maxAttempts = rateLimiterSection.GetValue<int>("MaxAttempts");
            var periodMinutes = rateLimiterSection.GetValue<int>("PeriodMinutes");

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
                    var httpContext = onRejectedContext.HttpContext;
                    httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                    double retryAfterSeconds = 0;
                    if (onRejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        retryAfterSeconds = Math.Ceiling(retryAfter.TotalSeconds);
                        httpContext.Response.Headers.RetryAfter = retryAfterSeconds.ToString(CultureInfo.InvariantCulture);
                    }

                    await httpContext.Response.WriteAsJsonAsync(new ApiResponse
                    {
                        Success = false,
                        Errors =
                        [
                            new ApiError
                            {
                                Code = AppError.Codes.TooManyRequests,
                                Message = $"Too many requests. Retry after {retryAfterSeconds:0} second(s)."
                            }
                        ]
                    }, cancellationToken);
                };
            });

            return services;
        }

        public IServiceCollection AddNativeOpenApi()
        {
            services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = "Pulse API",
                        Version = "v1",
                        Description = "API for testing and managing Pulse application.",
                        Contact = new OpenApiContact
                        {
                            Name = "Pulse Team",
                        }
                    };
                    return Task.CompletedTask;
                });

                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<BearerSecurityOperationTransformer>();
            });

            return services;
        }
    }

    private static string GetClientIdentifier(HttpContext context) =>
        context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
}
