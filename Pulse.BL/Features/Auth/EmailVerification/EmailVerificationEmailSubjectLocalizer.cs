using System.Globalization;
using System.Reflection;
using System.Resources;
using Pulse.BL.Common.Localization;

namespace Pulse.BL.Features.Auth.EmailVerification;

internal static class EmailVerificationEmailSubjectLocalizer
{
    private const string ResourceBaseName =
        "Pulse.BL.Features.Auth.EmailVerification.Resources.EmailVerificationEmailTexts";
    private const string SubjectKey = "EmailVerificationSubject";
    private static readonly ResourceManager ResourceManager =
        new(ResourceBaseName, typeof(EmailVerificationEmailSubjectLocalizer).GetTypeInfo().Assembly);

    internal static string GetSubject(string language)
    {
        string normalizedLanguage = LanguageTagNormalizer.NormalizePrimarySubtag(language);
        CultureInfo culture = CultureInfo.GetCultureInfo(
            LanguageCodes.Supported.Contains(normalizedLanguage) ? normalizedLanguage : LanguageCodes.English);

        return ResourceManager.GetString(SubjectKey, culture)
            ?? ResourceManager.GetString(SubjectKey, CultureInfo.GetCultureInfo(LanguageCodes.English))
            ?? "Verify your Pulse email address";
    }
}
