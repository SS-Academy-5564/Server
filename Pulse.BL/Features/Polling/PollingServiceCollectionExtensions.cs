using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.ManualTrigger;
using Pulse.BL.Features.Polling.Options;

namespace Pulse.BL.Features.Polling;

public static class PollingServiceCollectionExtensions
{
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

        services.AddSingleton<IManualCheckQueue>(_ => new ManualCheckQueue(capacity: 100));
        services.AddScoped<IManualCheckExecutor, ManualCheckExecutor>();
        services.AddScoped<IManualMonitorTriggerService, ManualMonitorTriggerService>();
        services.AddHostedService<ManualCheckHostedService>();

        return services;
    }
}
