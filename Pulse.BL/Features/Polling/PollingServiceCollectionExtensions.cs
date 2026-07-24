using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.ManualTrigger;
using Pulse.BL.Features.Polling.ManualTrigger.Execution;
using Pulse.BL.Features.Polling.ManualTrigger.Queue;
using Pulse.BL.Features.Polling.Options;

namespace Pulse.BL.Features.Polling;

public static class PollingServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core polling functionality: the monitor HTTP client,
    /// JSON path reader, and polling service. Required by any host that runs
    /// monitor checks — both Pulse.API (manual triggers) and Pulse.Worker
    /// (scheduled polling).
    /// </summary>
    public static IServiceCollection AddPolling(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHttpClient(HttpMonitorClient.ClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false
            });

        services.AddOptions<PollingWorkerOptions>()
            .Bind(configuration.GetRequiredSection(PollingWorkerOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<PollingWorkerOptions>, PollingWorkerOptionsValidator>();

        services.AddScoped<IPollingService, PollingService>();
        services.AddScoped<IHttpMonitorClient, HttpMonitorClient>();
        services.AddScoped<IJsonPathReader, JsonPathReader>();

        return services;
    }

    /// <summary>
    /// Registers manual monitor trigger functionality: the bounded check
    /// queue, its background executor, and the trigger service used by the
    /// "Run Check Now" endpoint. Only needed by hosts that expose the
    /// manual-trigger HTTP endpoint (Pulse.API). Requires <see cref="AddPolling"/>
    /// to have been called first, since it depends on <see cref="IPollingService"/>.
    /// </summary>
    public static IServiceCollection AddManualTrigger(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ManualCheckQueueOptions>()
            .Bind(configuration.GetRequiredSection(ManualCheckQueueOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IValidateOptions<ManualCheckQueueOptions>, ManualCheckQueueOptionsValidator>();

        services.AddSingleton<IManualCheckQueue, ManualCheckQueue>();
        services.AddScoped<IManualCheckExecutor, ManualCheckExecutor>();
        services.AddSingleton<IScopedManualCheckRunner, ScopedManualCheckRunner>();
        services.AddScoped<IManualMonitorTriggerService, ManualMonitorTriggerService>();
        services.AddHostedService<ManualCheckHostedService>();

        return services;
    }
}
