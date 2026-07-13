using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.Options;
using Pulse.DAL.DependencyInjection;
using Pulse.Worker.Polling;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context,services) =>
{
    services
        .AddHttpClient(HttpMonitorClient.ClientName)
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            AllowAutoRedirect = false
        });
    services.AddOptions<PollingWorkerOptions>()
        .Bind(context.Configuration.GetRequiredSection(PollingWorkerOptions.SectionName))
        .ValidateOnStart();

    services.AddSingleton<IValidateOptions<PollingWorkerOptions>, PollingWorkerOptionsValidator>();

    services.AddScoped<IPollingService, PollingService>();
    services.AddScoped<IHttpMonitorClient, HttpMonitorClient>();
    services.AddScoped<IJsonPathReader, JsonPathReader>();

    services.AddDataAccess();

    services.AddHostedService<PollerWorker>();

    services.AddHealthChecks();
});

builder.ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder.Configure(app =>
    {
        app.UseRouting();
        app.UseEndpoints(endpoints =>
            endpoints.MapHealthChecks("/health")
        );
    });
});

IHost host = builder.Build();
host.Run();
