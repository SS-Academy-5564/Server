using System.Globalization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Pulse.API.Common.Security.RateLimiting;
using Pulse.API.Constants;
using Pulse.API.Documentation;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;

namespace Pulse.API.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddPulseRateLimiting(IConfiguration configuration)
        {
            services.AddSingleton<IValidateOptions<RateLimitRuleOptions>, RateLimitRuleOptionsValidator>();
            services.AddOptions<RateLimitRuleOptions>(RateLimitSections.Login)
                .Bind(configuration.GetRequiredSection(RateLimitSections.Login))
                .ValidateOnStart();

            services.AddRateLimiter();
            services.AddOptions<RateLimiterOptions>()
                .Configure<IOptionsMonitor<RateLimitRuleOptions>>((rateLimiterOptions, rateLimitRules) =>
                {
                    rateLimiterOptions.AddPolicy(RateLimitPolicies.Login, context =>
                    {
                        RateLimitRuleOptions loginRateLimit = rateLimitRules.Get(RateLimitSections.Login);

                        return RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: GetClientIdentifier(context),
                            factory: _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = loginRateLimit.MaxAttempts,
                                TokensPerPeriod = 1,
                                ReplenishmentPeriod = TimeSpan.FromSeconds(
                                    loginRateLimit.PeriodMinutes * 60.0 / loginRateLimit.MaxAttempts),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            });
                    });

                    rateLimiterOptions.OnRejected = async (onRejectedContext, cancellationToken) =>
                    {
                        HttpContext httpContext = onRejectedContext.HttpContext;
                        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                        double retryAfterSeconds = 0;
                        if (onRejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
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
                                    Message = retryAfterSeconds > 0
                                        ? $"Too many requests. Retry after {retryAfterSeconds:0} second(s)"
                                        : "Too many requests."
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

    private static string GetClientIdentifier(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
}
