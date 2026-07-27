namespace Pulse.BL.Common.Localization;

/// <summary>
/// Utility methods for working with RFC-style language tags (e.g. "en", "en-US", "uk_UA").
/// </summary>
public static class LanguageTagNormalizer
{
    /// <summary>
    /// Normalizes a language tag to its primary subtag.
    /// Examples: "en-US" -> "en", "uk_UA" -> "uk".
    /// Returns an empty string for null, whitespace, wildcard, or invalid tags.
    /// </summary>
    public static string NormalizePrimarySubtag(string? languageTag)
    {
        if (string.IsNullOrWhiteSpace(languageTag))
        {
            return string.Empty;
        }

        string tag = languageTag.Trim().ToLowerInvariant();
        if (tag == "*")
        {
            return string.Empty;
        }

        string[] parts = tag.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return parts.Length == 0 ? string.Empty : parts[0];
    }
}
