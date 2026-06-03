using Pulse.DAL.DependencyInjection;
using Pulse.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDataAccess();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
