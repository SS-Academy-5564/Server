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
    /// <param name="languageTag">The raw language tag value (for example: "en", "en-US", "uk_UA")</param>
    /// <returns>
    /// The normalized primary subtag (for example: "en" or "uk")
    /// Returns <see cref="string.Empty"/> when the input is null, whitespace, wildcard, malformed,
    /// or when the primary subtag is invalid
    /// </returns>
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

        string[] parts = tag.Split(['-', '_'], StringSplitOptions.None);
        if (parts.Length == 0)
        {
            return string.Empty;
        }

        // Reject malformed tags like "uk--UA", "uk__UA", "-uk", "uk-" before normalization
        if (parts.Any(static p => string.IsNullOrWhiteSpace(p) || p.Any(char.IsWhiteSpace)))
        {
            return string.Empty;
        }

        string primary = parts[0];
        return IsValidPrimarySubtag(primary) ? primary : string.Empty;
    }

    private static bool IsValidPrimarySubtag(string value)
    {
        if (value.Length is < 2 or > 8)
        {
            return false;
        }

        return value.All(char.IsLetter);
    }
}
