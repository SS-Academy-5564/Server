using System.Globalization;
using Pulse.BL.Common.Localization;

namespace Pulse.API.Features.Auth.PasswordReset;

/// <summary>
/// Resolves the best-matching language for emails based on the Accept-Language HTTP header.
/// Implements RFC 7231 language preference parsing with quality values.
/// Falls back to English if no supported language is found.
/// </summary>
internal static class EmailLanguageResolver
{
    private static readonly string FallbackLanguage = LanguageCodes.English;
    private static readonly IReadOnlySet<string> SupportedLanguages = LanguageCodes.Supported;

    /// <summary>
    /// Resolves the best-matching email language from the Accept-Language header.
    /// Parses language preferences with quality values (RFC 7231) and returns the highest-priority
    /// supported language. Returns English if the header is missing or contains no supported languages.
    /// </summary>
    /// <param name="acceptLanguageHeader">The Accept-Language HTTP header value (e.g., "uk-UA,uk;q=0.9,en;q=0.8").</param>
    /// <returns>A supported language code ("en" or "uk").</returns>
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
            string segment = segments[i];
            if (string.IsNullOrWhiteSpace(segment))
            {
                continue;
            }

            string[] parts = segment.Split(';', StringSplitOptions.TrimEntries);
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
