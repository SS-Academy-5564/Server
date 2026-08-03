namespace Pulse.BL.Common.Localization;

/// <summary>
/// Well-known language codes used across the application for email localization.
/// </summary>
public static class LanguageCodes
{
    /// <summary>BCP 47 primary subtag for English.</summary>
    /// <returns>"en"</returns>
    public const string English = "en";

    /// <summary>BCP 47 primary subtag for Ukrainian.</summary>
    /// <returns>"uk"</returns>
    public const string Ukrainian = "uk";

    /// <summary>All language codes supported for email content localization.</summary>
    /// <returns>A read-only set containing <see cref="English"/> and <see cref="Ukrainian"/>.</returns>
    public static IReadOnlySet<string> Supported { get; } = new HashSet<string>(StringComparer.Ordinal) { English, Ukrainian };
}
