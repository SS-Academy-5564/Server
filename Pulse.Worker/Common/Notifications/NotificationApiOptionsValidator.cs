using Microsoft.Extensions.Options;

namespace Pulse.Worker.Common.Notifications;

public sealed class NotificationApiOptionsValidator : IValidateOptions<NotificationApiOptions>
{
    private readonly IHostEnvironment _environment;

    public NotificationApiOptionsValidator(IHostEnvironment environment)
    {
        _environment = environment;
    }

    public ValidateOptionsResult Validate(string? name, NotificationApiOptions options)
    {
        List<string> errors = new();

        if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out Uri? apiUri))
        {
            errors.Add($"{NotificationApiOptions.SectionName}:ApiBaseUrl must be an absolute  URL.");
        }

        bool isHttps = apiUri!.Scheme == Uri.UriSchemeHttps;
        bool isDevelopmentHttp = _environment.IsDevelopment() && apiUri.Scheme == Uri.UriSchemeHttp;

        if (!isHttps && !isDevelopmentHttp)
        {
            errors.Add($"{NotificationApiOptions.SectionName}:ApiBaseUrl must be an https URL.");
        }

        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            errors.Add($"{NotificationApiOptions.SectionName}:ApiKey is required.");
        }

        return errors.Count == 0 ? ValidateOptionsResult.Success : ValidateOptionsResult.Fail(errors);
    }
}
