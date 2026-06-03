using DbUp;
using System.Reflection;

namespace Pulse.DAL.Database;

public static class DatabaseInitializer
{
    private static readonly string[] MigrationFolders =
    [
        ".Scripts.Tables.",
        ".Scripts.Indexes.",
        ".Scripts.Seed.",
    ];

    private const string DevSeedFolder = ".Scripts.Seed.Dev.";

    public static void RunMigrations(string connectionString, bool seedDevData = false)
    {
        EnsureDatabase.For.SqlDatabase(connectionString);

        foreach (var folder in MigrationFolders)
        {
            RunScripts(connectionString, folder);
        }

        if (seedDevData)
        {
            RunScripts(connectionString, DevSeedFolder);
        }
    }

    private static void RunScripts(
        string connectionString,
        string folderFilter)
    {
        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                scriptName =>
                {
                    if (!scriptName.Contains(folderFilter, StringComparison.OrdinalIgnoreCase))
                        return false;

                    if (folderFilter == DevSeedFolder)
                        return true;

                    return !scriptName.Contains(DevSeedFolder, StringComparison.OrdinalIgnoreCase);
                })
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new Exception(
                $"Database migration failed ({folderFilter}): {result.Error}",
                result.Error);
        }
    }


}
