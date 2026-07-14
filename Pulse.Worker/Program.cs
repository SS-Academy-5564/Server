using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Pulse.DAL.DependencyInjection;
using Pulse.Worker;

IHostBuilder builder = Host.CreateDefaultBuilder(args);

builder.ConfigureServices(services =>
{
    services.AddDataAccess();
    services.AddHostedService<Worker>();

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
