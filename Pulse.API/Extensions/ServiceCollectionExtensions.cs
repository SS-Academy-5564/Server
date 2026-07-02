using System.Globalization;
using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Pulse.API.Common.Security.RateLimiting;
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

        public IServiceCollection AddPulseRateLimiting(IConfiguration configuration)
        {
            services.AddSingleton<IValidateOptions<RateLimitRuleOptions>, RateLimitRuleOptionsValidator>();
            services.AddSingleton<IValidateOptions<SlidingWindowRateLimitRuleOptions>, SlidingWindowRateLimitRuleOptionsValidator>();

            services.AddOptions<RateLimitRuleOptions>(RateLimitSections.Login)
                .Bind(configuration.GetRequiredSection(RateLimitSections.Login))
                .ValidateOnStart();

            services.AddOptions<SlidingWindowRateLimitRuleOptions>(RateLimitSections.PasswordReset)
                .Bind(configuration.GetRequiredSection(RateLimitSections.PasswordReset))
                .ValidateOnStart();

            services.AddRateLimiter();
            services.AddOptions<RateLimiterOptions>()
                .Configure<
                    IOptionsMonitor<RateLimitRuleOptions>,
                    IOptionsMonitor<SlidingWindowRateLimitRuleOptions>>(
                    (rateLimiterOptions, rateLimitRules, slidingWindowRules) =>
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

                    rateLimiterOptions.AddPolicy(RateLimitPolicies.PasswordReset, context =>
                    {
                        SlidingWindowRateLimitRuleOptions resetRateLimit = slidingWindowRules.Get(RateLimitSections.PasswordReset);

                        return RateLimitPartition.GetSlidingWindowLimiter(
                            partitionKey: GetClientIdentifier(context),
                            factory: _ => new SlidingWindowRateLimiterOptions
                            {
                                PermitLimit = resetRateLimit.MaxAttempts,
                                Window = TimeSpan.FromMinutes(resetRateLimit.PeriodMinutes),
                                SegmentsPerWindow = resetRateLimit.Segments,
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
