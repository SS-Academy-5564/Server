using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.BackgroundTasks;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.ManualTrigger;
using Pulse.BL.Features.Polling.Options;

namespace Pulse.BL.Features.Polling;

public static class PollingServiceCollectionExtensions
{
    /// <summary>
    /// Registers everything needed to run and trigger monitor checks:
    /// the HTTP client, JSON path reader, polling service, manual trigger
    /// service, and the background task queue used to run manual checks
    /// without blocking the HTTP request.
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

        services.AddSingleton<IBackgroundTaskQueue>(_ => new BackgroundTaskQueue(capacity: 100));
        services.AddHostedService<QueuedHostedService>();
        services.AddScoped<IManualMonitorTriggerService, ManualMonitorTriggerService>();

        return services;
    }
}
