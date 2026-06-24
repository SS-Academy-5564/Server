using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Pulse.API.Constants;
using Pulse.API.Documentation;
using Pulse.API.Responses;
using Pulse.BL.Common.Errors;
using Pulse.BL.Common.Security.Tokens;

namespace Pulse.API.Extensions;

public static class ServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddJwtAuthentication()
        {
            services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer();

            services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
                .Configure<IOptions<JwtOptions>>((bearerOptions, jwtOptionsAccessor) =>
                {
                    JwtOptions jwtOptions = jwtOptionsAccessor.Value;
                    bearerOptions.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtOptions.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtOptions.Audience,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),

                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                    };
                });

            return services;
        }

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
