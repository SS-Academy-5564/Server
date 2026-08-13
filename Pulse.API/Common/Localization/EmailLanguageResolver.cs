using System.Globalization;
using Pulse.BL.Common.Localization;

namespace Pulse.API.Common.Localization;

/// <summary>
/// Resolves the best-matching supported email language from an Accept-Language header.
/// </summary>
internal static class EmailLanguageResolver
{
    private static readonly string FallbackLanguage = LanguageCodes.English;
    private static readonly IReadOnlySet<string> SupportedLanguages = LanguageCodes.Supported;

    /// <summary>
    /// Resolves the highest-priority supported language or English when no supported language is present.
    /// </summary>
    /// <param name="acceptLanguageHeader">The Accept-Language header value.</param>
    /// <returns>A supported primary language code.</returns>
    public static string Resolve(string? acceptLanguageHeader)
    {
        if (string.IsNullOrWhiteSpace(acceptLanguageHeader))
        {
            return FallbackLanguage;
        }

        LanguagePreference? bestMatch = null;

        foreach (LanguagePreference candidate in ParsePreferences(acceptLanguageHeader))
        {
            if (!SupportedLanguages.Contains(candidate.Language))
            {
                continue;
            }

            if (bestMatch is null || candidate.IsHigherPriorityThan(bestMatch))
            {
                bestMatch = candidate;
            }
        }

        return bestMatch?.Language ?? FallbackLanguage;
    }

    private static IEnumerable<LanguagePreference> ParsePreferences(string header)
    {
        string[] segments = header.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        for (int i = 0; i < segments.Length; i++)
        {
            string[] parts = segments[i].Split(';', StringSplitOptions.TrimEntries);
            if (parts.Length == 0)
            {
                continue;
            }

            string normalizedLanguage = LanguageTagNormalizer.NormalizePrimarySubtag(parts[0]);
            if (string.IsNullOrWhiteSpace(normalizedLanguage))
            {
                continue;
            }

            double quality = 1.0;
            for (int p = 1; p < parts.Length; p++)
            {
                if (!parts[p].StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string qValue = parts[p][2..].Trim();
                if (!double.TryParse(qValue, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out double parsedQ)
                    || parsedQ is < 0 or > 1)
                {
                    quality = -1;
                    break;
                }

                quality = parsedQ;
            }

            if (quality <= 0)
            {
                continue;
            }

            yield return new LanguagePreference(normalizedLanguage, quality, i);
        }
    }

    private sealed record LanguagePreference(string Language, double Quality, int Index)
    {
        public bool IsHigherPriorityThan(LanguagePreference other)
        {
            if (Quality > other.Quality)
            {
                return true;
            }

            if (Quality < other.Quality)
            {
                return false;
            }

            return Index < other.Index;
        }
    }
}
