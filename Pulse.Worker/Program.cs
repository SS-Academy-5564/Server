using Microsoft.Extensions.Options;
using Pulse.DAL.DependencyInjection;
using Pulse.Worker.Polling;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDataAccess();
builder.Services.AddSingleton<IValidateOptions<PollingWorkerOptions>, PollingWorkerOptionsValidator>();
builder.Services.AddOptions<PollingWorkerOptions>()
    .Bind(builder.Configuration.GetRequiredSection(PollingWorkerOptions.SectionName))
    .ValidateOnStart();

builder.Services.AddHostedService<PollerWorker>();

IHost host = builder.Build();
host.Run();
