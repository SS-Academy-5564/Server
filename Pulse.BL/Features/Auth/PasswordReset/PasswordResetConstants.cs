using Pulse.BL.Common.Localization;

namespace Pulse.BL.Features.Auth.PasswordReset;

/// <summary>
/// Constants for password reset localization and configuration.
/// </summary>
internal static class PasswordResetConstants
{
    /// <summary>
    /// Supported email languages for password reset notifications.
    /// </summary>
    public static class SupportedLanguages
    {
        /// <summary>BCP 47 primary subtag for English.</summary>
        /// <returns>"en"</returns>
        public static string English => LanguageCodes.English;

        /// <summary>BCP 47 primary subtag for Ukrainian.</summary>
        /// <returns>"uk"</returns>
        public static string Ukrainian => LanguageCodes.Ukrainian;

        /// <summary>All language codes supported for password reset email notifications.</summary>
        /// <returns>A read-only set of supported language codes.</returns>
        public static IReadOnlySet<string> All => LanguageCodes.Supported;
    }
}
