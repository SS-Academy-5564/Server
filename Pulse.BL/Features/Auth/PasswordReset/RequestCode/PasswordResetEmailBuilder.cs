using System.Reflection;
using Pulse.BL.Common.Localization;
using Scriban;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Builds localized email content for password reset OTP codes.
/// Supports English and Ukrainian with automatic template loading and rendering.
/// </summary>
internal static class PasswordResetEmailBuilder
{
    private const string ResourcePrefix = "Pulse.BL.Features.Auth.PasswordReset.RequestCode.PasswordResetEmail";
    private static readonly IReadOnlyDictionary<string, TemplateSet> TemplatesByLanguage;

    static PasswordResetEmailBuilder()
    {
        TemplatesByLanguage = new Dictionary<string, TemplateSet>(StringComparer.Ordinal)
        {
            [PasswordResetConstants.SupportedLanguages.English] = new(
                LoadTemplate(BuildResourceName("", "html")),
                LoadTemplate(BuildResourceName("", "txt"))),
            [PasswordResetConstants.SupportedLanguages.Ukrainian] = new(
                LoadTemplate(BuildResourceName(".ukrainian", "html")),
                LoadTemplate(BuildResourceName(".ukrainian", "txt")))
        };
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
        => ResolveTemplateSet(language).Html.Render(new { code, code_ttl_minutes = codeTtlMinutes });

    /// <summary>
    /// Builds the plain text email body for the specified language.
    /// </summary>
    /// <param name="code">The 6-digit password reset code.</param>
    /// <param name="codeTtlMinutes">The code expiration time in minutes.</param>
    /// <param name="language">The language code (e.g., "en", "uk", "uk-UA"). Defaults to English if unsupported.</param>
    /// <returns>The rendered plain text email body.</returns>
    public static string BuildPlainTextBody(string code, int codeTtlMinutes, string language)
        => ResolveTemplateSet(language).PlainText.Render(new { code, code_ttl_minutes = codeTtlMinutes });

    private static string BuildResourceName(string languageSuffix, string extension)
        => $"{ResourcePrefix}{languageSuffix}.{extension}";

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

    private static TemplateSet ResolveTemplateSet(string language)
    {
        string normalizedLanguage = LanguageTagNormalizer.NormalizePrimarySubtag(language);
        if (string.IsNullOrWhiteSpace(normalizedLanguage))
        {
            normalizedLanguage = PasswordResetConstants.SupportedLanguages.English;
        }

        if (TemplatesByLanguage.TryGetValue(normalizedLanguage, out TemplateSet? templateSet))
        {
            return templateSet;
        }

        return TemplatesByLanguage[PasswordResetConstants.SupportedLanguages.English];
    }

    private sealed record TemplateSet(Template Html, Template PlainText);
}
