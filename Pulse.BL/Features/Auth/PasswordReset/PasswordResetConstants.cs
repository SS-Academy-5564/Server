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
        public static string English => LanguageCodes.English;
        public static string Ukrainian => LanguageCodes.Ukrainian;

        /// <summary>All supported language codes (delegates to <see cref="LanguageCodes.Supported"/>).</summary>
        public static IReadOnlySet<string> All => LanguageCodes.Supported;
    }
}
