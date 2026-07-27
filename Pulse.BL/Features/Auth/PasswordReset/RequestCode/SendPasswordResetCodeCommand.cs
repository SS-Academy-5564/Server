using Pulse.BL.Features.Auth.PasswordReset;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Command to request a password reset code for a user account.
/// The backend will send a 6-digit OTP code via email in the specified language.
/// Always returns success to prevent email enumeration attacks.
/// </summary>
/// <param name="Email">The email address of the account requesting a password reset.</param>
/// <param name="Language">
/// The email language code derived from the Accept-Language header (normalized to primary subtag).
/// Must be one of the supported languages: "en" (English) or "uk" (Ukrainian).
/// Defaults to "en" if the header contains no supported languages.
/// </param>
public sealed record SendPasswordResetCodeCommand(string Email, string Language)
{
    /// <summary>
    /// Validates that the language code is one of the supported languages.
    /// </summary>
    /// <returns>True if the language is supported; otherwise, false.</returns>
    public bool IsLanguageSupported()
    {
        string normalizedLanguage = NormalizeLanguage(Language);
        return PasswordResetConstants.SupportedLanguages.All.Contains(normalizedLanguage);
    }

    private static string NormalizeLanguage(string language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return string.Empty;
        }

        string[] parts = language.Trim().ToLowerInvariant()
            .Split(['-', '_'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return parts.Length == 0 ? string.Empty : parts[0];
    }
}
