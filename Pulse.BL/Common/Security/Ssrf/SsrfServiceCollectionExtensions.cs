using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Polling.Http;

namespace Pulse.BL.Common.Security.Ssrf;

/// <summary>
/// Provides extension methods for registering SSRF protection services into the DI container.
/// </summary>
public static class SsrfServiceCollectionExtensions
{
    /// <summary>
    /// Registers SSRF protection: options (validated on start), the guard, the
    /// DNS resolver, and the connection factory used by the polling HTTP client.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSsrfProtection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<SsrfProtectionOptions>, SsrfProtectionOptionsValidator>();
        services.AddOptions<SsrfProtectionOptions>()
            .Bind(configuration.GetRequiredSection(SsrfProtectionOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<ISsrfGuard, SsrfGuard>();
        services.AddSingleton<IDnsResolver, SystemDnsResolver>();
        services.AddSingleton<SsrfConnectionFactory>();

        return services;
    }
}
