using DbUp;
using System.Reflection;

namespace Pulse.DAL.Database;

public static class DatabaseInitializer
{
    private static readonly string[] MigrationFolders =
   [
      ".Scripts.Tables.",
      ".Scripts.Seed.",
      ".Scripts.Indexes."
   ];

    public static void RunMigrations(string connectionString)
    {
        EnsureDatabase.For.SqlDatabase(connectionString);

        foreach (var folder in MigrationFolders)
        {
            RunScripts(connectionString, folder);
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
                scriptName => scriptName.Contains(
                    folderFilter,
                    StringComparison.OrdinalIgnoreCase))
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
