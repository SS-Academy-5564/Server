using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Features.Polling.Http;

namespace Pulse.BL.Common.Security.Ssrf;

public static class SsrfServiceCollectionExtensions
{
    /// <summary>
    /// Registers SSRF protection: options (validated on start), the guard, the
    /// DNS resolver, and the connection factory used by the polling HTTP client.
    /// </summary>
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
