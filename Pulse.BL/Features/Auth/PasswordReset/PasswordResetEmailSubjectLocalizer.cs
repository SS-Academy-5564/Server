using System.Globalization;
using System.Reflection;
using System.Resources;

namespace Pulse.BL.Features.Auth.PasswordReset;

/// <summary>
/// Resolves localized subjects for password reset emails from resource files.
/// Uses English as the fallback language for missing or unsupported cultures.
/// </summary>
internal static class PasswordResetEmailSubjectLocalizer
{
    private const string ResourceBaseName = "Pulse.BL.Features.Auth.PasswordReset.Resources.PasswordResetEmailTexts";
    private const string SubjectKey = "PasswordResetSubject";

    private static readonly ResourceManager ResourceManager =
        new(ResourceBaseName, typeof(PasswordResetEmailSubjectLocalizer).GetTypeInfo().Assembly);

    public static string GetSubject(string language)
    {
        CultureInfo culture = ResolveCulture(language);

        string? localized = ResourceManager.GetString(SubjectKey, culture);
        if (!string.IsNullOrWhiteSpace(localized))
        {
            return localized;
        }

        // Final safety net if resources are misconfigured.
        return ResourceManager.GetString(SubjectKey, CultureInfo.GetCultureInfo(PasswordResetConstants.SupportedLanguages.English))
            ?? "Your Pulse password reset code";
    }

    private static CultureInfo ResolveCulture(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return CultureInfo.GetCultureInfo(PasswordResetConstants.SupportedLanguages.English);
        }

        string normalized = language.Trim().ToLowerInvariant();
        string[] parts = normalized.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        string primaryLanguage = parts.Length == 0 ? PasswordResetConstants.SupportedLanguages.English : parts[0];

        return PasswordResetConstants.SupportedLanguages.All.Contains(primaryLanguage)
            ? CultureInfo.GetCultureInfo(primaryLanguage)
            : CultureInfo.GetCultureInfo(PasswordResetConstants.SupportedLanguages.English);
    }
}
