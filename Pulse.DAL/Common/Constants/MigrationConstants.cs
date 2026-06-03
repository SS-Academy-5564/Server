namespace Pulse.DAL.Common.Constants;

internal static class MigrationConstants
{
    public static readonly string[] Folders =
    [
        ".Scripts.Tables.",
        ".Scripts.Indexes.",
        ".Scripts.Seed.",
    ];

    public const string DevSeedFolder = ".Scripts.Seed.Dev.";
}