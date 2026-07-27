using Pulse.BL.Common.Localization;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Command to request a password reset code for a user account.
/// The backend will send a 6-digit OTP code via email in the specified language.
/// Always returns success to prevent email enumeration attacks.
/// </summary>
/// <param name="Email">The email address of the account requesting a password reset.</param>
/// <param name="Language">
/// The email language code derived from the Accept-Language header (e.g. "uk-UA").
/// Only the primary subtag is used for matching (e.g. "uk").
/// Supported languages are "en" (English) and "uk" (Ukrainian); English is used as fallback.
/// </param>
public sealed record SendPasswordResetCodeCommand(string Email, string Language)
{
    /// <summary>
    /// Validates that the language code is one of the supported languages.
    /// </summary>
    /// <returns>True if the language is supported; otherwise, false.</returns>
    public bool IsLanguageSupported()
    {
        string normalizedLanguage = LanguageTagNormalizer.NormalizePrimarySubtag(Language);
        return PasswordResetConstants.SupportedLanguages.All.Contains(normalizedLanguage);
    }
}
