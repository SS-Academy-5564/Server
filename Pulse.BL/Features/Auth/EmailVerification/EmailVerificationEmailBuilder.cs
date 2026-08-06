using System.Net;
using System.Reflection;
using Pulse.BL.Common.Localization;
using Scriban;

namespace Pulse.BL.Features.Auth.EmailVerification;

internal static class EmailVerificationEmailBuilder
{
    private const string ResourcePrefix = "Pulse.BL.Features.Auth.EmailVerification.EmailVerificationEmail";
    private static readonly IReadOnlyDictionary<string, TemplateSet> TemplatesByLanguage =
        new Dictionary<string, TemplateSet>(StringComparer.Ordinal)
        {
            [LanguageCodes.English] = new(
                LoadTemplate(BuildResourceName("", "html")),
                LoadTemplate(BuildResourceName("", "txt"))),
            [LanguageCodes.Ukrainian] = new(
                LoadTemplate(BuildResourceName(".ukrainian", "html")),
                LoadTemplate(BuildResourceName(".ukrainian", "txt")))
        };

    internal static string BuildSubject(string language)
        => EmailVerificationEmailSubjectLocalizer.GetSubject(language);

    internal static string BuildHtmlBody(string verificationUrl, int tokenLifetimeHours, string language)
        => ResolveTemplateSet(language).Html.Render(new
        {
            verification_url = WebUtility.HtmlEncode(verificationUrl),
            token_lifetime_hours = tokenLifetimeHours,
            hour_label = BuildHourLabel(tokenLifetimeHours, language)
        });

    internal static string BuildPlainTextBody(string verificationUrl, int tokenLifetimeHours, string language)
        => ResolveTemplateSet(language).PlainText.Render(new
        {
            verification_url = verificationUrl,
            token_lifetime_hours = tokenLifetimeHours,
            hour_label = BuildHourLabel(tokenLifetimeHours, language)
        });

    internal static string BuildVerificationUrl(string verificationPageUrl, string token)
    {
        UriBuilder builder = new(verificationPageUrl);
        string existingQuery = builder.Query.TrimStart('?');
        string tokenParameter = $"token={Uri.EscapeDataString(token)}";
        builder.Query = string.IsNullOrEmpty(existingQuery)
            ? tokenParameter
            : $"{existingQuery}&{tokenParameter}";

        return builder.Uri.AbsoluteUri;
    }

    private static string BuildResourceName(string languageSuffix, string extension)
        => $"{ResourcePrefix}{languageSuffix}.{extension}";

    private static string BuildHourLabel(int hours, string language)
    {
        if (ResolveLanguage(language) == LanguageCodes.English)
        {
            return hours == 1 ? "hour" : "hours";
        }

        int lastTwoDigits = hours % 100;
        if (lastTwoDigits is >= 11 and <= 14)
        {
            return "годин";
        }

        return (hours % 10) switch
        {
            1 => "годину",
            2 or 3 or 4 => "години",
            _ => "годин"
        };
    }

    private static TemplateSet ResolveTemplateSet(string language)
        => TemplatesByLanguage[ResolveLanguage(language)];

    private static string ResolveLanguage(string language)
    {
        string normalizedLanguage = LanguageTagNormalizer.NormalizePrimarySubtag(language);
        return TemplatesByLanguage.ContainsKey(normalizedLanguage)
            ? normalizedLanguage
            : LanguageCodes.English;
    }

    private static Template LoadTemplate(string resourceName)
    {
        Assembly assembly = typeof(EmailVerificationEmailBuilder).Assembly;
        using Stream stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded email template '{resourceName}' was not found.");
        using StreamReader reader = new(stream);
        Template template = Template.Parse(reader.ReadToEnd());

        if (template.HasErrors)
        {
            throw new InvalidOperationException(
                $"Email template '{resourceName}' is invalid: {string.Join("; ", template.Messages)}");
        }

        return template;
    }

    private sealed record TemplateSet(Template Html, Template PlainText);
}
