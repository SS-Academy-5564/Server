using Microsoft.Extensions.Options;

namespace Pulse.API.Filters.InternalNotificatiom;

public sealed class InternalNotificationOptionsValidator : IValidateOptions<InternalNotificationOptions>
{
    public ValidateOptionsResult Validate(string? name, InternalNotificationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return string.IsNullOrWhiteSpace(options.ApiKey)
            ? ValidateOptionsResult.Fail($"{InternalNotificationOptions.SectionName}:ApiKey is required.")
            : ValidateOptionsResult.Success;
    }
}
