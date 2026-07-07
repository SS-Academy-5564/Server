using Microsoft.Extensions.Options;

namespace Pulse.BL.Features.Polling;

/// <summary>
/// Validates polling worker configuration values.
/// </summary>
public sealed class PollingWorkerOptionsValidator : IValidateOptions<PollingWorkerOptions>
{
    /// <summary>
    /// Validates the configured polling worker options.
    /// </summary>
    /// <param name="name">The named options instance being validated.</param>
    /// <param name="options">The polling worker options.</param>
    /// <returns>The validation result.</returns>
    public ValidateOptionsResult Validate(string? name, PollingWorkerOptions options)
    {
        List<string> errors = new();
        string optionsPath = string.IsNullOrWhiteSpace(name) ? PollingWorkerOptions.SectionName : name;

        if (options.BatchSize <= 0)
        {
            errors.Add($"{optionsPath}:BatchSize must be greater than zero.");
        }

        if (options.LoopIntervalSeconds is < 1 or > 60)
        {
            errors.Add($"{optionsPath}:LoopIntervalSeconds must be between 1 and 60 seconds.");
        }

        if (options.MaxConcurrentRequests <= 0)
        {
            errors.Add($"{optionsPath}:MaxConcurrentRequests must be greater than zero.");
        }

        return errors.Count > 0
            ? ValidateOptionsResult.Fail(errors)
            : ValidateOptionsResult.Success;
    }
}
