using Pulse.DAL.DependencyInjection;
using Pulse.Worker;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDataAccess();

builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
host.Run();
