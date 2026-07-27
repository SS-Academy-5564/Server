using System.Reflection;
using Scriban;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Builds localized email content for password reset OTP codes.
/// Supports English and Ukrainian with automatic template loading and rendering.
/// </summary>
internal static class PasswordResetEmailBuilder
{
    private static readonly Template EnglishHtmlTemplate;
    private static readonly Template EnglishPlainTextTemplate;
    private static readonly Template UkrainianHtmlTemplate;
    private static readonly Template UkrainianPlainTextTemplate;

    static PasswordResetEmailBuilder()
    {
        EnglishHtmlTemplate = LoadTemplate("Pulse.BL.Features.Auth.PasswordReset.RequestCode.PasswordResetEmail.html");
        EnglishPlainTextTemplate = LoadTemplate("Pulse.BL.Features.Auth.PasswordReset.RequestCode.PasswordResetEmail.txt");
        UkrainianHtmlTemplate = LoadTemplate("Pulse.BL.Features.Auth.PasswordReset.RequestCode.PasswordResetEmail.ukrainian.html");
        UkrainianPlainTextTemplate = LoadTemplate("Pulse.BL.Features.Auth.PasswordReset.RequestCode.PasswordResetEmail.ukrainian.txt");
    }

    /// <summary>
    /// Builds the email subject line for the specified language.
    /// </summary>
    /// <param name="language">The language code (e.g., "en", "uk", "uk-UA"). Defaults to English if unsupported.</param>
    /// <returns>The localized email subject.</returns>
    public static string BuildSubject(string language)
        => PasswordResetEmailSubjectLocalizer.GetSubject(language);

    /// <summary>
    /// Builds the HTML email body for the specified language.
    /// </summary>
    /// <param name="code">The 6-digit password reset code.</param>
    /// <param name="codeTtlMinutes">The code expiration time in minutes.</param>
    /// <param name="language">The language code (e.g., "en", "uk", "uk-UA"). Defaults to English if unsupported.</param>
    /// <returns>The rendered HTML email body.</returns>
    public static string BuildHtmlBody(string code, int codeTtlMinutes, string language)
        => GetHtmlTemplate(language).Render(new { code, code_ttl_minutes = codeTtlMinutes });

    /// <summary>
    /// Builds the plain text email body for the specified language.
    /// </summary>
    /// <param name="code">The 6-digit password reset code.</param>
    /// <param name="codeTtlMinutes">The code expiration time in minutes.</param>
    /// <param name="language">The language code (e.g., "en", "uk", "uk-UA"). Defaults to English if unsupported.</param>
    /// <returns>The rendered plain text email body.</returns>
    public static string BuildPlainTextBody(string code, int codeTtlMinutes, string language)
        => GetPlainTextTemplate(language).Render(new { code, code_ttl_minutes = codeTtlMinutes });

    private static Template LoadTemplate(string resourceName)
    {
        Assembly assembly = typeof(PasswordResetEmailBuilder).Assembly;

        using Stream? stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null)
        {
            throw new FileNotFoundException($"Could not find embedded resource '{resourceName}'.");
        }

        using var reader = new StreamReader(stream);
        return Template.Parse(reader.ReadToEnd());
    }

    private static Template GetHtmlTemplate(string language)
        => IsUkrainian(language) ? UkrainianHtmlTemplate : EnglishHtmlTemplate;

    private static Template GetPlainTextTemplate(string language)
        => IsUkrainian(language) ? UkrainianPlainTextTemplate : EnglishPlainTextTemplate;

    private static bool IsUkrainian(string language)
        => string.Equals(NormalizeLanguageTag(language), PasswordResetConstants.SupportedLanguages.Ukrainian, StringComparison.Ordinal);

    /// <summary>
    /// Normalizes a language tag to its primary language subtag (first segment before '-' or '_').
    /// Examples: "en-US" → "en", "uk_UA" → "uk", "invalid" → "invalid"
    /// </summary>
    private static string NormalizeLanguageTag(string language)
    {
        string normalized = language.Trim().ToLowerInvariant();
        string[] parts = normalized.Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return PasswordResetConstants.SupportedLanguages.English;
        }

        return parts[0];
    }
}
