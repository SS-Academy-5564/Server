using System.Net;
using System.Reflection;
using Scriban;

namespace Pulse.BL.Features.Auth.EmailVerification;

internal static class EmailVerificationEmailBuilder
{
    private const string HtmlResourceName =
        "Pulse.BL.Features.Auth.EmailVerification.EmailVerificationEmail.html";
    private const string PlainTextResourceName =
        "Pulse.BL.Features.Auth.EmailVerification.EmailVerificationEmail.txt";
    private static readonly Template HtmlTemplate = LoadTemplate(HtmlResourceName);
    private static readonly Template PlainTextTemplate = LoadTemplate(PlainTextResourceName);

    internal static string BuildSubject() => "Verify your Pulse email address";

    internal static string BuildHtmlBody(string verificationUrl, int tokenLifetimeHours)
        => HtmlTemplate.Render(new
        {
            verification_url = WebUtility.HtmlEncode(verificationUrl),
            token_lifetime_hours = tokenLifetimeHours,
            hour_label = tokenLifetimeHours == 1 ? "hour" : "hours"
        });

    internal static string BuildPlainTextBody(string verificationUrl, int tokenLifetimeHours)
        => PlainTextTemplate.Render(new
        {
            verification_url = verificationUrl,
            token_lifetime_hours = tokenLifetimeHours,
            hour_label = tokenLifetimeHours == 1 ? "hour" : "hours"
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
}
