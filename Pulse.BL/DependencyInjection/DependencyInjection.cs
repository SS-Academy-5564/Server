using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Security.Ssrf;
using Pulse.BL.Common.Security.Tokens;
using Pulse.BL.Features.Auth;
using Pulse.BL.Features.Auth.Login.LoginLockout;
using Pulse.BL.Features.Auth.PasswordReset;
using Pulse.BL.Features.Email;
using Pulse.BL.Features.Organization;
using Pulse.BL.Features.Polling;

namespace Pulse.BL.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogic(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHandlersFromAssembly(Assembly.GetExecutingAssembly());
        services.AddSingleton(TimeProvider.System);

        services.AddSingleton<IValidateOptions<LoginLockoutOptions>, LoginLockoutOptionsValidator>();
        services.AddOptions<LoginLockoutOptions>()
            .Bind(configuration.GetRequiredSection(LoginLockoutOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<JwtOptions>, JwtOptionsValidator>();
        services
            .AddOptions<JwtOptions>()
            .Bind(configuration.GetRequiredSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<RefreshTokenOptions>, RefreshTokenOptionsValidator>();
        services
            .AddOptions<RefreshTokenOptions>()
            .Bind(configuration.GetRequiredSection(RefreshTokenOptions.SectionName))
            .ValidateOnStart();

        services.AddTransient<IRefreshTokenService, RefreshTokenService>();

        services.AddSingleton<IValidateOptions<PasswordResetOptions>, PasswordResetOptionsValidator>();
        services
            .AddOptions<PasswordResetOptions>()
            .Bind(configuration.GetRequiredSection(PasswordResetOptions.SectionName))
            .ValidateOnStart();

        services.AddAuth(configuration);
        services.AddEmailing(configuration);
        services.AddPolling();
        services.AddManualCheck(configuration);

        services.AddScoped<CreateOrganizationHandler>();

        services.AddSsrfProtection(configuration);

        return services;
    }
}
