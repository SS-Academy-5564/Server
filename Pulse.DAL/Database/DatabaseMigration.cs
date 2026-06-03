using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Pulse.DAL.Database;

public class DatabaseMigration : IHostedLifecycleService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseMigration> _logger;

    public DatabaseMigration(IConfiguration configuration, ILogger<DatabaseMigration> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartingAsync(CancellationToken ct)
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string is missing.");

        _logger.LogInformation("Running database migrations...");

        var retries = 5;
        for (var i = 1; i <= retries; i++)
        {
            try
            {
                DatabaseInitializer.RunMigrations(connectionString);
                _logger.LogInformation("Database migrations completed successfully.");
                return;
            }
            catch (Exception ex) when (i < retries)
            {
                var delay = TimeSpan.FromSeconds(i * 2);

                _logger.LogWarning(
                    ex,
                    "Migration attempt {Attempt}/{Total} failed, retrying in {Delay}s",
                    i,
                    retries,
                    delay.TotalSeconds);

                await Task.Delay(delay, ct);
            }
        }
    }

    public Task StartedAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StartAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StoppedAsync(CancellationToken ct) => Task.CompletedTask;
    public Task StoppingAsync(CancellationToken ct) => Task.CompletedTask;
}