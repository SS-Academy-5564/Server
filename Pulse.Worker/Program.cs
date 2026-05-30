using Pulse.DAL.Database;
using Pulse.DAL.DependencyInjection;
using Pulse.Worker;

var builder = Host.CreateApplicationBuilder(args);

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Connection string 'DefaultConnection' not found.");

DatabaseInitializer.RunMigrations(connectionString);

builder.Services.AddDataAccess(builder.Configuration);

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
