using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Common.Security.Ssrf;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.Http;
using Pulse.BL.Features.Polling.Options;
using Pulse.DAL.DependencyInjection;
using Pulse.Worker.Polling;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddSsrfProtection(context.Configuration);

    services
        .AddHttpClient(HttpMonitorClient.ClientName)
        .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
        {
            SsrfConnectionFactory connectionFactory = serviceProvider.GetRequiredService<SsrfConnectionFactory>();

            return new SocketsHttpHandler
            {
                AllowAutoRedirect = false,
                // Disable connection reuse so the SSRF connect callback re-resolves
                // and re-validates the destination on every poll (defeats DNS rebinding).
                PooledConnectionLifetime = TimeSpan.Zero,
                ConnectCallback = connectionFactory.ConnectAsync
            };
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
