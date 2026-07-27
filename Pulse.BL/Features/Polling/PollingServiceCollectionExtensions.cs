using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.ManualCheck;
using Pulse.BL.Features.Polling.ManualCheck.Queue;
using Pulse.BL.Features.Polling.Options;

namespace Pulse.BL.Features.Polling;

public static class PollingServiceCollectionExtensions
{
    public static IServiceCollection AddPolling(this IServiceCollection services)
    {
        services
            .AddHttpClient(HttpMonitorClient.ClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false
            });

        services.AddScoped<IPollingService, PollingService>();
        services.AddScoped<IHttpMonitorClient, HttpMonitorClient>();
        services.AddScoped<IJsonPathReader, JsonPathReader>();

        return services;
    }

    public static IServiceCollection AddPollingWorkerOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<PollingWorkerOptions>()
            .Bind(configuration.GetRequiredSection(PollingWorkerOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<PollingWorkerOptions>, PollingWorkerOptionsValidator>();

        return services;
    }

    public static IServiceCollection AddManualCheck(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ManualCheckQueueOptions>()
            .Bind(configuration.GetRequiredSection(ManualCheckQueueOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ManualCheckQueueOptions>, ManualCheckQueueOptionsValidator>();

        services.AddSingleton<IManualCheckQueue, ManualCheckQueue>();
        services.AddHostedService<ManualCheckQueueWorker>();

        return services;
    }
}
