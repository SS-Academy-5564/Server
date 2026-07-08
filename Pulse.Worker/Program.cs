using Microsoft.Extensions.Options;
using Pulse.BL.Common.Helpers.Json;
using Pulse.BL.Features.Polling;
using Pulse.BL.Features.Polling.Http;
using Pulse.DAL.DependencyInjection;
using Pulse.Worker.Polling;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDataAccess();

builder.Services
    .AddHttpClient(HttpMonitorClient.ClientName)
    .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        AllowAutoRedirect = false
    });

builder.Services.AddScoped<IPollingService, PollingService>();
builder.Services.AddScoped<IHttpMonitorClient, HttpMonitorClient>();
builder.Services.AddScoped<IJsonPathReader, JsonPathReader>();

builder.Services.AddSingleton<IValidateOptions<PollingWorkerOptions>, PollingWorkerOptionsValidator>();
builder.Services.AddOptions<PollingWorkerOptions>()
    .Bind(builder.Configuration.GetRequiredSection(PollingWorkerOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddHostedService<PollerWorker>();

IHost host = builder.Build();
host.Run();
