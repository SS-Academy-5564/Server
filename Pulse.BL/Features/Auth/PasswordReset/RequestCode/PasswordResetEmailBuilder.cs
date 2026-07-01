using System.Reflection;
using Scriban;

namespace Pulse.BL.Features.Auth.PasswordReset.RequestCode;

/// <summary>
/// Builds the email content for a password reset OTP code.
/// </summary>
internal static class PasswordResetEmailBuilder
{
    private static readonly Template HtmlTemplate;
    private static readonly Template PlainTextTemplate;

    static PasswordResetEmailBuilder()
    {
        Assembly assembly = typeof(PasswordResetEmailBuilder).Assembly;

        using Stream? htmlStream = assembly.GetManifestResourceStream("Pulse.BL.Features.Auth.PasswordReset.RequestCode.PasswordResetEmail.html");
        if (htmlStream == null)
        {
            throw new FileNotFoundException("Could not find embedded resource for PasswordResetEmail.html");
        }

        using var htmlReader = new StreamReader(htmlStream);
        HtmlTemplate = Template.Parse(htmlReader.ReadToEnd());

        using Stream? txtStream = assembly.GetManifestResourceStream("Pulse.BL.Features.Auth.PasswordReset.RequestCode.PasswordResetEmail.txt");
        if (txtStream == null)
        {
            throw new FileNotFoundException("Could not find embedded resource for PasswordResetEmail.txt");
        }

        using var txtReader = new StreamReader(txtStream);
        PlainTextTemplate = Template.Parse(txtReader.ReadToEnd());
    }

    public static string BuildSubject() => "Your Pulse password reset code";

    public static string BuildHtmlBody(string code, int codeTtlMinutes)
        => HtmlTemplate.Render(new { code, code_ttl_minutes = codeTtlMinutes });

    public static string BuildPlainTextBody(string code, int codeTtlMinutes)
        => PlainTextTemplate.Render(new { code, code_ttl_minutes = codeTtlMinutes });
}
