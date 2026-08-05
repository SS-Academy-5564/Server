using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Pulse.API.Common.Notifications;
using Pulse.API.Common.Security;
using Pulse.API.Common.Security.RateLimiting;
using Pulse.API.Constants;
using Pulse.API.Documentation;
using Pulse.API.Responses;
using Pulse.BL.Common.Notifications;
using Pulse.BL.Common.Security;
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
                    bearerOptions.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            string? accessToken = context.Request.Query["access_token"];

                            if (!string.IsNullOrWhiteSpace(accessToken) &&
                                context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            return services;
        }

        public IServiceCollection AddCurrentUserService()
        {
            services.AddHttpContextAccessor();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
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

            services.AddOptions<RateLimitRuleOptions>(RateLimitSections.Refresh)
                .Bind(configuration.GetRequiredSection(RateLimitSections.Refresh))
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

                    rateLimiterOptions.AddPolicy(RateLimitPolicies.Refresh, context =>
                    {
                        RateLimitRuleOptions refreshRateLimit = rateLimitRules.Get(RateLimitSections.Refresh);

                        return RateLimitPartition.GetTokenBucketLimiter(
                            partitionKey: GetClientIdentifier(context),
                            factory: _ => new TokenBucketRateLimiterOptions
                            {
                                TokenLimit = refreshRateLimit.MaxAttempts,
                                TokensPerPeriod = 1,
                                ReplenishmentPeriod = TimeSpan.FromSeconds(
                                    refreshRateLimit.PeriodMinutes * 60.0 / refreshRateLimit.MaxAttempts),
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

                    rateLimiterOptions.AddPolicy(RateLimitPolicies.ManualMonitorTrigger, httpContext =>
                    {
                        string monitorId = httpContext.Request.RouteValues["id"]?.ToString() ?? "unknown";

                        return RateLimitPartition.GetFixedWindowLimiter(monitorId, _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 1,
                            Window = TimeSpan.FromSeconds(30),
                            QueueLimit = 0
                        });
                    });

                    rateLimiterOptions.OnRejected = async (onRejectedContext, cancellationToken) =>
                    {
                        HttpContext httpContext = onRejectedContext.HttpContext;
                        httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                        httpContext.Response.ContentType = "application/json";

                        string retryMessage = onRejectedContext.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter)
                            ? $"Please try again in {retryAfter.TotalSeconds:F0} seconds."
                            : "Please wait before trying again.";

                        await httpContext.Response.WriteAsJsonAsync(new ApiResponse
                        {
                            Success = false,
                            Errors =
                            [
                                new ApiError
                                {
                                    Code = RateLimitErrorCodes.RateLimited,
                                    Message = $"Manual check was already triggered recently. {retryMessage}"
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

        /// <summary>
        /// Registers the SignalR services and notification service used by Pulse.
        /// </summary>
        /// <returns>The service collection so that additional registrations can be chained.</returns>
        public IServiceCollection AddPulseSignalR()
        {
            services.AddTransient<INotificationService, SignalrNotificationService>();
            services.AddSignalR();

            return services;
        }
    }

    private static string GetClientIdentifier(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
}
