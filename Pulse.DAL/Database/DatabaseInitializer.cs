using DbUp;
using System.Reflection;

namespace Pulse.DAL.Database
{
    public static class DatabaseInitializer
    {
        public static void RunMigrations(string connectionString)
        {
            EnsureDatabase.For.SqlDatabase(connectionString);

            RunScripts(connectionString, ".tables.");
            RunScripts(connectionString, ".indexes.");
        }

        private static void RunScripts(string connectionString, string filter)
        {
            var upgrader = DeployChanges.To
                .SqlDatabase(connectionString)
                .WithScriptsEmbeddedInAssembly(
                    Assembly.GetExecutingAssembly(),
                    name => name.ToLower().Contains(filter))
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                throw new Exception(
                    $"Database migration failed ({filter}): {result.Error.Message}",
                    result.Error);
            }
        }
    }
}
