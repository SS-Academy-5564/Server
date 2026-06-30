using Microsoft.Extensions.Options;

namespace Pulse.API.Common.Security.RateLimiting;

/// <summary>
/// Validates rate limiting rule configuration values.
/// </summary>
public sealed class RateLimitRuleOptionsValidator : IValidateOptions<RateLimitRuleOptions>
{
    /// <summary>
    /// Validates the configured rate limiting rule.
    /// </summary>
    /// <param name="name">The named options instance being validated.</param>
    /// <param name="options">The rate limiting rule options.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, RateLimitRuleOptions options)
    {
        List<string> errors = new();
        string optionsPath = string.IsNullOrWhiteSpace(name) ? "RateLimit" : name;

        if (options.MaxAttempts <= 0)
        {
            errors.Add($"{optionsPath}:MaxAttempts must be greater than zero.");
        }

        if (options.PeriodMinutes <= 0)
        {
            errors.Add($"{optionsPath}:PeriodMinutes must be greater than zero.");
        }

        if (options.Segments <= 0)
        {
            errors.Add($"{optionsPath}:Segments must be greater than zero.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
