using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Pulse.BL.Features.Polling;
using Pulse.DAL.DependencyInjection;
using Pulse.Worker.Polling;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices((context, services) =>
{
    services.AddDataAccess();
    services.AddPolling(context.Configuration);

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
