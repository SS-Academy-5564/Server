using Microsoft.Extensions.Options;

namespace Pulse.Worker.Common.Notifications;

/// <summary>
/// Validates options for internal notification API settings.
/// </summary>
public sealed class NotificationApiOptionsValidator : IValidateOptions<NotificationApiOptions>
{
    private readonly IHostEnvironment _environment;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationApiOptionsValidator"/> class.
    /// </summary>
    /// <param name="environment">The hosting environment instance.</param>
    public NotificationApiOptionsValidator(IHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// Validates the specified <see cref="NotificationApiOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> representing validation success or failure.</returns>
    public ValidateOptionsResult Validate(string? name, NotificationApiOptions options)
    {
        List<string> errors = new();

        if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out Uri? apiUri))
        {
            errors.Add($"{NotificationApiOptions.SectionName}:ApiBaseUrl must be an absolute URL.");
            return ValidateOptionsResult.Fail(errors);
        }

        bool isHttps = apiUri.Scheme == Uri.UriSchemeHttps;
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
