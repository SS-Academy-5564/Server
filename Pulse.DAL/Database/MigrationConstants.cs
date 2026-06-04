namespace Pulse.DAL.Database;

internal static class MigrationConstants
{
    public static readonly string[] MigrationFolders = 
    [
        ".Scripts.Tables.",
        ".Scripts.Indexes.",
        ".Scripts.Seed.",
    ];
    public const string DevSeedFolder = ".Scripts.Seed.Dev.";
}