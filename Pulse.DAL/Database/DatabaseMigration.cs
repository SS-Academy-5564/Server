using Microsoft.Extensions.Logging;

namespace Pulse.DAL.Database;

public static class DatabaseMigration
{
    public static async Task RunWithRetryAsync(
        string connectionString,
        ILogger logger,
        bool seedDevData = false,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'MigrationConnection' is missing or empty.");
        }

        logger.LogInformation("Running database migrations...");

        const int retries = 5;
        for (int i = 1; i <= retries; i++)
        {
            try
            {
                DatabaseInitializer.RunMigrations(connectionString, seedDevData);
                logger.LogInformation("Database migrations completed successfully.");
                return;
            }
            catch (Exception ex) when (i < retries)
            {
                var delay = TimeSpan.FromSeconds(i * 2);

                logger.LogWarning(
                    ex,
                    "Migration attempt {Attempt}/{Total} failed, retrying in {Delay}s",
                    i,
                    retries,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }
}
