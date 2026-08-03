namespace Pulse.BL.Common.Localization;

/// <summary>
/// Well-known language codes used across the application for email localization.
/// </summary>
public static class LanguageCodes
{
    public const string English = "en";
    public const string Ukrainian = "uk";

    /// <summary>All language codes supported for email content.</summary>
    public static IReadOnlySet<string> Supported { get; } = new HashSet<string>(StringComparer.Ordinal) { English, Ukrainian };
}
