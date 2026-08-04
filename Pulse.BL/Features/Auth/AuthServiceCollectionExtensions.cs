using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Security.Passwords;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth.EmailVerification;
using Pulse.BL.Features.Auth.Login.LoginLockout;
using Pulse.BL.Features.Auth.PasswordReset;

namespace Pulse.BL.Features.Auth;

/// <summary>
/// Registers authentication-related services and options for the business layer.
/// </summary>
public static class AuthServiceCollectionExtensions
{
    /// <summary>
    /// Registers password hashing, JWT token generation, lockout logic, and auth-related options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddTransient<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddTransient<IEmailVerificationTokenService, EmailVerificationTokenService>();
        services.AddScoped<ILoginLockoutService, LoginLockoutService>();

        services.AddSingleton<IValidateOptions<LoginLockoutOptions>, LoginLockoutOptionsValidator>();
        services.AddOptions<LoginLockoutOptions>()
            .Bind(configuration.GetRequiredSection(LoginLockoutOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetRequiredSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<PasswordResetOptions>, PasswordResetOptionsValidator>();
        services
            .AddOptions<PasswordResetOptions>()
            .Bind(configuration.GetRequiredSection(PasswordResetOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<EmailVerificationOptions>, EmailVerificationOptionsValidator>();
        services
            .AddOptions<EmailVerificationOptions>()
            .Bind(configuration.GetRequiredSection(EmailVerificationOptions.SectionName))
            .ValidateOnStart();

        return services;
    }
}
