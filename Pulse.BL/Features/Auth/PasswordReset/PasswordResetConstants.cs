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
        public const string English = "en";
        public const string Ukrainian = "uk";

        /// <summary>
        /// Gets all supported language codes.
        /// </summary>
        public static IReadOnlySet<string> All { get; } = new HashSet<string> { English, Ukrainian };
    }

    /// <summary>
    /// English localization strings.
    /// </summary>
    public static class EmailTexts
    {
        public static class English
        {
            public const string Subject = "Your Pulse password reset code";
        }

        public static class Ukrainian
        {
            public const string Subject = "Код для скидання пароля Pulse";
        }
    }
}
