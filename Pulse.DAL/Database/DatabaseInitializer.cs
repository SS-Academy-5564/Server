using System.Reflection;
using DbUp;
using DbUp.Engine;
using Pulse.DAL.Common.Constants;
using Pulse.DAL.Exceptions;

namespace Pulse.DAL.Database;

public static class DatabaseInitializer
{
    public static void RunMigrations(string connectionString, bool seedDevData = false)
    {
        EnsureDatabase.For.SqlDatabase(connectionString);

        foreach (string folder in MigrationConstants.Folders)
        {
            RunScripts(connectionString, folder);
        }

        if (seedDevData)
        {
            RunScripts(connectionString, MigrationConstants.DevSeedFolder);
        }
    }

    private static void RunScripts(
        string connectionString,
        string folderFilter)
    {
        UpgradeEngine upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(
                Assembly.GetExecutingAssembly(),
                scriptName =>
                {
                    if (!scriptName.Contains(folderFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        return false;
                    }

                    if (folderFilter == MigrationConstants.DevSeedFolder)
                    {
                        return true;
                    }

                    return !scriptName.Contains(MigrationConstants.DevSeedFolder, StringComparison.OrdinalIgnoreCase);
                })
            .LogToConsole()
            .Build();

        DatabaseUpgradeResult result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new MigrationFailedException(
                $"Database migration failed ({folderFilter}). See inner exception for details.",
                result.Error);
        }
    }
}
