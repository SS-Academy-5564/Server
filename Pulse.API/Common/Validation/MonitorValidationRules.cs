using FluentValidation;
using Pulse.BL.Features.Monitors;

namespace Pulse.API.Common.Validation;

/// <summary>
/// Provides reusable validation rules for monitor domain properties.
/// </summary>
public static class MonitorValidationRules
{
    private const int MinPollingIntervalSeconds = 60;
    private const int MaxPollingIntervalSeconds = 24 * 60 * 60;
    private const int MinPollingTimeoutSeconds = 5;
    private const int MaxPollingTimeoutSeconds = 30;

    private static readonly string[] AllowedHttpMethods =
    [
        "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS"
    ];

    private static readonly MonitorStatus[] AllowedStatuses =
    [
        MonitorStatus.Enabled,
        MonitorStatus.Disabled
    ];

    /// <summary>
    /// Applies validation rules for the monitor name property.
    /// Ensures the name is not empty and does not exceed 64 characters.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the target property.</param>
    /// <returns>The <see cref="IRuleBuilderOptions{T, Property}"/> instance allowing further rule chaining.</returns>
    public static IRuleBuilderOptions<T, string> ApplyMonitorNameRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Monitor name is required.")
            .MaximumLength(64).WithMessage("Monitor name must be at most 64 characters.");
    }

    /// <summary>
    /// Applies validation rules for the monitor endpoint URL.
    /// Ensures the URL is not empty, does not exceed 2083 characters, and is a valid absolute HTTP/HTTPS address.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the target property.</param>
    /// <returns>The <see cref="IRuleBuilderOptions{T, Property}"/> instance allowing further rule chaining.</returns>
    public static IRuleBuilderOptions<T, string> ApplyUrlRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Endpoint URL is required.")
            .MaximumLength(2083).WithMessage("Endpoint URL must be at most 2083 characters.")
            .Must(BeAValidHttpUrl).WithMessage("Endpoint URL must be a valid HTTP or HTTPS URL.");
    }

    /// <summary>
    /// Applies validation rules for the HTTP request method.
    /// Ensures the method is not empty and belongs to the set of supported HTTP verbs.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the target property.</param>
    /// <returns>The <see cref="IRuleBuilderOptions{T, Property}"/> instance allowing further rule chaining.</returns>
    public static IRuleBuilderOptions<T, string> ApplyHttpMethodRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Request method is required.")
            .Must(method => AllowedHttpMethods.Contains(method, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Request method must be one of: {string.Join(", ", AllowedHttpMethods)}.");
    }

    /// <summary>
    /// Applies validation rules for the JSON path expression used to extract payload values.
    /// Ensures the path is not empty and does not exceed 255 characters.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the target property.</param>
    /// <returns>The <see cref="IRuleBuilderOptions{T, Property}"/> instance allowing further rule chaining.</returns>
    public static IRuleBuilderOptions<T, string> ApplyResultPathRules<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Result path is required.")
            .MaximumLength(255).WithMessage("Result path must be at most 255 characters.");
    }

    /// <summary>
    /// Applies validation rules for the monitor status property.
    /// Ensures the status is a valid enum value and is restricted to Enabled or Disabled.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the target property.</param>
    /// <returns>The <see cref="IRuleBuilderOptions{T, Property}"/> instance allowing further rule chaining.</returns>
    public static IRuleBuilderOptions<T, MonitorStatus> ApplyStatusRules<T>(this IRuleBuilder<T, MonitorStatus> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum().WithMessage("Status must be a valid monitor status.")
            .Must(status => AllowedStatuses.Contains(status))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}.");
    }

    /// <summary>
    /// Applies validation rules for the polling interval in seconds.
    /// Ensures the interval is between 60 seconds (1 minute) and 86,400 seconds (24 hours).
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the target property.</param>
    /// <returns>The <see cref="IRuleBuilderOptions{T, Property}"/> instance allowing further rule chaining.</returns>
    public static IRuleBuilderOptions<T, int> ApplyPollingIntervalRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(MinPollingIntervalSeconds, MaxPollingIntervalSeconds)
            .WithMessage("Polling interval must be between 1 minute and 24 hours.");
    }

    /// <summary>
    /// Applies validation rules for the HTTP request timeout in seconds.
    /// Ensures the timeout is between 5 and 30 seconds.
    /// </summary>
    /// <typeparam name="T">The type of the object being validated.</typeparam>
    /// <param name="ruleBuilder">The rule builder for the target property.</param>
    /// <returns>The <see cref="IRuleBuilderOptions{T, Property}"/> instance allowing further rule chaining.</returns>
    public static IRuleBuilderOptions<T, int> ApplyPollingTimeoutRules<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .InclusiveBetween(MinPollingTimeoutSeconds, MaxPollingTimeoutSeconds)
            .WithMessage("Polling timeout must be between 5 and 30 seconds.");
    }

    private static bool BeAValidHttpUrl(string? url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
