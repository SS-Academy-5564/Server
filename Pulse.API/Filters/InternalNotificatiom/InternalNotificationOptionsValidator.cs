using Microsoft.Extensions.Options;

namespace Pulse.API.Filters.InternalNotificatiom;

/// <summary>
/// Validates options for internal notification authentication.
/// </summary>
public sealed class InternalNotificationOptionsValidator : IValidateOptions<InternalNotificationOptions>
{
    /// <summary>
    /// Validates the specified <see cref="InternalNotificationOptions"/> instance.
    /// </summary>
    /// <param name="name">The name of the options instance being validated.</param>
    /// <param name="options">The options instance to validate.</param>
    /// <returns>A <see cref="ValidateOptionsResult"/> representing validation success or failure.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="options"/> is <see langword="null"/>.</exception>
    public ValidateOptionsResult Validate(string? name, InternalNotificationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return string.IsNullOrWhiteSpace(options.ApiKey)
            ? ValidateOptionsResult.Fail($"{InternalNotificationOptions.SectionName}:ApiKey is required.")
            : ValidateOptionsResult.Success;
    }
}
