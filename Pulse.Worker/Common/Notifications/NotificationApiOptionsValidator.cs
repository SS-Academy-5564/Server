using Microsoft.Extensions.Options;

namespace Pulse.Worker.Common.Notifications;

public sealed class NotificationApiOptionsValidator : IValidateOptions<NotificationApiOptions>
{
    public ValidateOptionsResult Validate(string? name, NotificationApiOptions options)
    {
        if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out Uri? apiUri) ||
            apiUri.Scheme is not "https")
        {
            return ValidateOptionsResult.Fail(
                $"{NotificationApiOptions.SectionName}:ApiBaseUrl must be an absolute HTTPS URL.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            return ValidateOptionsResult.Fail(
                $"{NotificationApiOptions.SectionName}:ApiKey is required.");
        }

        return ValidateOptionsResult.Success;
    }
}
